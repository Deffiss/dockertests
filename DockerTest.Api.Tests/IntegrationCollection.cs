using Xunit;

namespace DockerTest.Api.Tests
{
    [CollectionDefinition("IntegrationCollection")]
    public class IntegrationCollection : ICollectionFixture<IntegrationFixture>
    {
    }
}