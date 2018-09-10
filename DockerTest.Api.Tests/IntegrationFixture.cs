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

        private readonly TestContainer[] _containers;

        public HttpClient Client { get; }

        public IntegrationFixture()
        {
            // 1. Prepare all the containers with required software.
            _containers = CreateDockerEnvironment();

            // 2. Set connection string as environment variable. This will override appsettings.json value.
            Environment.SetEnvironmentVariable("ConnectionStrings__PersonsContext", GetTestDatabaseConnectionString(_containers[0]));

            // 3. Run in-memory test server to host our API.
            _server = CreateTestServer();

            // 4. Arrange some testing data in our database.
            ArrangeTestData();

            // 5. Create HTTP Client pointed to in-memory hosted API to use it in our tests.
            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        private TestContainer[] CreateDockerEnvironment(bool useContainerIpAddresses = false)
        {
            var containers = new TestContainer[]
            {
                new MsSqlTestContainer(MsSqlSaPassword, "mssql_" + ContainerNameSuffix, useContainerIpAddresses, Console.WriteLine),
                // Here we can have more containers
            };

            var tasks = containers.Select(container => container.Run()).ToArray();

            Task.WhenAll(tasks).Wait();

            return containers;
        }

        private TestServer CreateTestServer()
        {
            // Run in-memory Test host
            var startupAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath("", startupAssembly);

            var builder = WebHost.CreateDefaultBuilder().UseContentRoot(contentRoot).UseEnvironment("Test").UseStartup<Startup>();

            return new TestServer(builder);
        }

        private void ArrangeTestData()
        {
            using (var scope = _server.Host.Services.CreateScope())
            {
                var tests = GetType().Assembly.GetTypes().Where(t => t.Name.EndsWith("tests", StringComparison.OrdinalIgnoreCase));

                var testDataFunctions = tests.Select(t => t.GetMethod("ArrangeData"));
                foreach (var testDataFunction in testDataFunctions)
                {
                    ((Task)testDataFunction.Invoke(null, new[] { scope.ServiceProvider })).Wait();
                }
            }
        }

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

        private string GetTestDatabaseConnectionString(TestContainer container, bool useContainerIpAddresses = false) =>
            $"Data Source={(useContainerIpAddresses ? container.IpAddress : "localhost")}, {(useContainerIpAddresses ? 1433 : container.Ports[1433])}; Initial Catalog=PersonsTestDB; UID=sa; pwd={MsSqlSaPassword}; Application Name=Persons API; MultipleActiveResultSets=True;";
    }
}