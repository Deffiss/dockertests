using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DockerTest.Api.Tests
{
    public abstract class TestBase
    {
        private readonly IServiceProvider _serviceProvider;

        protected TestBase(IntegrationFixture integrationFixture)
        {
            Client = integrationFixture.Client;
            _serviceProvider = integrationFixture.ServiceProvider;
        }

        protected HttpClient Client { get; }

        protected T GetService<T>() => _serviceProvider.GetService<T>();
    }
}