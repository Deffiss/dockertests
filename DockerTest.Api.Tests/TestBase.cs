using System.Net.Http;

namespace DockerTest.Api.Tests
{
    public abstract class TestBase
    {
        protected TestBase(IntegrationFixture integrationFixture)
        {
            Client = integrationFixture.Client;
        }

        protected HttpClient Client { get; }
    }
}