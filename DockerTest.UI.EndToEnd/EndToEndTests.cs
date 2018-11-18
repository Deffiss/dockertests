using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mssql;
using Xunit;

namespace DockerTest.UI.EndToEnd
{
    public class EndToEndTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public EndToEndTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange database and selenuim containers
            var infrastructure = new DockerEnvironmentBuilder()
                .SetName("env")
                .AddMssqlContainer("mssql", "saPassword123")
                .AddContainer("webdriver", "selenium/standalone-chrome", "3.141.59-antimony")
                .Build();

            await infrastructure.Up();

            // Arrange container with the app inside
            var app = new DockerEnvironmentBuilder()
                .SetName("app")
                .AddContainer("app", "dockertestui", "latest", new Dictionary<string, string>
                {
                    ["ConnectionStrings__SampleDB"] = GetConnectionString(infrastructure)
                }, containerWaiter: new FuncContainerWaiter(CheckAppReadiness))
                .Build();

            await app.Up();

            // Connect to remote Chrome driver
            using (var driver = new RemoteWebDriver(
                new Uri(GetChromeDriverUrl(infrastructure)),
                new ChromeOptions { }))
            {
                // Go to home page and get all <tr> elements to count
                driver.Navigate().GoToUrl(GetAppUrl(app));
                var trs = driver.FindElementByTagName("tbody").FindElements(By.TagName("tr"));

                // Assert
                Assert.Equal(15, trs.Count);
            }

            // Cleanup
            app.Dispose();
            infrastructure.Dispose();
        }

        private static string GetAppUrl(DockerEnvironment app) => $"http://{app.GetContainer("app").IPAddress}";

        private static string GetChromeDriverUrl(DockerEnvironment dockerEnvironment) =>
            $"http://localhost:{dockerEnvironment.GetContainer("webdriver").Ports[4444]}/wd/hub";

        private static string GetConnectionString(DockerEnvironment dockerEnvironment)
        {
            var iPAddress = dockerEnvironment.GetContainer("mssql").IPAddress;
            var connectionString = $"Data Source={iPAddress}, 1433;" +
                $"Initial Catalog=Sample; UID=sa; pwd=saPassword123;" +
                $"Application Name=Sample App; MultipleActiveResultSets=True;";
            return connectionString;
        }

        private static async Task<bool> CheckAppReadiness(Container c)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"http://localhost:{c.Ports[80]}");
                    return response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
