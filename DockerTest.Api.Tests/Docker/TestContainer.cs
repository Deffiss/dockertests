using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerTest.Api.Tests.Docker
{
    public class TestContainer : IDisposable
    {
        private readonly DockerClient _docker;

        private readonly string _tag;

        private readonly string _imageName;

        private readonly string[] _environmentVariables;

        protected Action<string> Logger { get; }

        public string Name { get; }

        public string Id { get; private set; }

        public string IpAddress { get; private set; }

        public IDictionary<ushort, ushort> Ports { get; private set; }

        public virtual int AttemptsCount => 60;

        public virtual int DelayTime => 1000;

        public TestContainer(string dockerUrl, string imageName, string containerName, string tag = "latest", string[] environmentVariables = null,  Action<string> logger = null)
            : this(imageName, containerName, tag, environmentVariables, logger)
        {
            _docker = new DockerClientConfiguration(new Uri(dockerUrl)).CreateClient() ?? throw new ArgumentNullException(nameof(dockerUrl));
        }

        public TestContainer(string imageName, string containerName, string tag = "latest", string[] environmentVariables = null, Action<string> logger = null)
        {
            _imageName = imageName ?? throw new ArgumentNullException(nameof(imageName));
            _tag = tag;
            _environmentVariables = environmentVariables;
            Logger = logger;
            Name = containerName;
            _docker = _docker ?? new DockerClientConfiguration(new Uri(GetDockerDefaultUrl())).CreateClient() ?? throw new ArgumentNullException();
        }

        public async Task Run()
        {
            await Run(_imageName, _tag, _environmentVariables);
            Logger?.Invoke($"Waiting for container '{Name}'");
            await Wait();
            Logger?.Invoke($"Container '{Name}' is ready for use");
        }

        public void Dispose()
        {
            _docker.Containers.RemoveContainerAsync(Id, new ContainerRemoveParameters { Force = true });
            _docker.Dispose();
        }

        protected virtual Task Wait()
        {
            return Task.CompletedTask;
        }

        private async Task Run(string imageName, string tag, IList<string> env)
        {
            // Create container name
            Logger?.Invoke($"Container name: {Name}");

            // Try to find container in docker session
            var containers = await _docker.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            var startedContainer = containers.FirstOrDefault(c => c.Names.Contains($"/{Name}"));

            // If container already exist - remove that
            if (startedContainer != null)
            {
                await _docker.Containers.RemoveContainerAsync(startedContainer.ID, new ContainerRemoveParameters { Force = true });
            }

            var images = await _docker.Images.ListImagesAsync(new ImagesListParameters
            {
                All = true,
                MatchName = $"{imageName}:{tag}"
            });

            // If image not pulled yet - pull this.
            if (!images.Any())
            {
                await _docker.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageName,
                        Tag = tag
                    },
                    null,
                    new Progress<JSONMessage>(m => Logger?.Invoke($"Pulling image {imageName}:{tag}:\n{m.ProgressMessage}")));
            }

            // Create new container
            var container = await _docker.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Name = Name,
                    Image = $"{imageName}:{tag}",
                    AttachStdout = true,
                    Env = env,
                    Hostname = Name,
                    Domainname = Name,
                    HostConfig = new HostConfig
                    {
                        PublishAllPorts = true,
                    }
                }, CancellationToken.None);

            // Run container
            await _docker.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());

            // Try to find container in docker session
            containers = await _docker.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            startedContainer = containers.FirstOrDefault(c => c.Names.Contains($"/{Name}"));

            Logger?.Invoke($"Container state: {startedContainer?.State}");
            Logger?.Invoke($"Container status: {startedContainer?.Status}");
            Logger?.Invoke($"Container IPAddress: {startedContainer?.NetworkSettings.Networks.FirstOrDefault().Key} - {startedContainer?.NetworkSettings.Networks.FirstOrDefault().Value.IPAddress}");
            Id = container.ID;
            IpAddress = startedContainer?.NetworkSettings.Networks.FirstOrDefault().Value.IPAddress;
            Ports = startedContainer?.Ports.ToDictionary(p => p.PrivatePort, p => p.PublicPort);
        }

        private string GetDockerDefaultUrl()
        {
            var dockerHostVar = Environment.GetEnvironmentVariable("DOCKER_HOST");
            var defaultDockerUrl =
                !string.IsNullOrEmpty(dockerHostVar)
                    ? dockerHostVar
                    : !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "unix:///var/run/docker.sock"
                        : "npipe://./pipe/docker_engine";
            Logger?.Invoke($"Docker URL is:'{defaultDockerUrl}'");
            return defaultDockerUrl;
        }
    }
}
