using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DockerTest.Api.Tests.Docker;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace DockerTest.Api.Tests
{
    public class IntegrationFixture : IDisposable
    {
        private const string SolutionName = "DockerTest.sln";

        private const string ContainerNameSuffix = "show_tests";
        private const string MsSqlSaPassword = "SecurePassword123";

        private readonly TestServer _server;

        private readonly List<TestContainer> _containers;

        public IntegrationFixture()
        {
            Console.WriteLine("Start docker containers");

            // This is workaround in case if this is Docker-in-Docker (Gitlab Environment);
            var useContainerIpAddresses = bool.Parse(Environment.GetEnvironmentVariable("USE_CONTAINER_IPADDRESSES") ?? bool.FalseString);

            Console.WriteLine($"Use container IPAddresses: {useContainerIpAddresses}");

            // Now prepare all the containers.
            _containers = PrepareDockerEnvironment(useContainerIpAddresses).Result.ToList();

            Console.WriteLine("Docker containers started");

            // Set connection string as environment variable. This will override appsettings.json value.
            Environment.SetEnvironmentVariable("ConnectionStrings__PersonsContext", GetTestDatabaseConnectionString(useContainerIpAddresses, _containers[0]));

            // Run in-memory Test host
            var startupAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath("", startupAssembly);

            var builder = WebHost.CreateDefaultBuilder().UseContentRoot(contentRoot).UseEnvironment("Test").UseStartup<Startup>();

            _server = new TestServer(builder);

            // It's time to call ArrangeData methods to prepare our tests.
            using (var scope = _server.Host.Services.CreateScope())
            {
                ServiceProvider = scope.ServiceProvider;
                var tests = GetType().Assembly.GetTypes().Where(t => t.Name.EndsWith("tests", StringComparison.OrdinalIgnoreCase));

                var testDataFunctions = tests.Select(t => t.GetMethod("ArrangeData"));
                foreach (var testDataFunction in testDataFunctions)
                {
                    ((Task)testDataFunction.Invoke(null, new[] { scope.ServiceProvider })).Wait();
                }
            }

            ServiceProvider = _server.Host.Services;

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            _server.Dispose();
            foreach (var container in _containers)
            {
                container.Dispose();
            }

            Client.Dispose();
        }

        private string GetProjectPath(string solutionRelativePath, Assembly startupAssembly) =>
            Path.GetFullPath(Path.Combine(GetSolutionPath(), solutionRelativePath, startupAssembly.GetName().Name));

        private string GetSolutionPath()
        {
            // Get currently executing test project path
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;

            // Find the folder which contains the solution file. We then use this information to find the target
            // project which we want to test.
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, SolutionName));
                if (solutionFileInfo.Exists)
                {
                    return solutionFileInfo.Directory.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Solution root could not be located using application root {applicationBasePath}.");
        }

        private async Task<IEnumerable<TestContainer>> PrepareDockerEnvironment(bool useContainerIpAddresses)
        {
            var containers = new List<TestContainer>
            {
                new MsSqlTestContainer(MsSqlSaPassword, "mssql_" + ContainerNameSuffix, useContainerIpAddresses, Console.WriteLine)
            };

            var tasks = containers.Select(container => container.Run()).ToArray();

            await Task.WhenAll(tasks);

            return containers;
        }

        private string GetTestDatabaseConnectionString(bool useContainerIpAddresses, TestContainer container) =>
            $"Data Source={(useContainerIpAddresses ? container.IpAddress : "localhost")}, {(useContainerIpAddresses ? 1433 : container.Ports[1433])}; Initial Catalog=PersonsTestDB; UID=sa; pwd={MsSqlSaPassword}; Application Name=Persons API; MultipleActiveResultSets=True;";
    }
}