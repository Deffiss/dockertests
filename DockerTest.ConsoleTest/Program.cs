using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTest.ConsoleTest
{
    class Program
    {
        async static Task Main(string[] args)
        {
            // Get the default URL for docker daemon (assuming it is on the localhost).
            var defaultDockerUrl = GetDockerDefaultUrl();

            // Create docker client.
            var docker = new DockerClientConfiguration(new Uri(defaultDockerUrl)).CreateClient();

            // Use monster method to run the container in a safe way.
            var (containerName, containerId, ipAddress, ports) = await RunContainer(
                docker,
                imageName: "microsoft/mssql-server-linux",
                tag: "latest",
                containerName: "my-msssql",
                "ACCEPT_EULA=Y", "SA_PASSWORD=SecurePassword123");

            Console.WriteLine($"{containerName} container has been run: {containerId}");
        }

        private static async Task<(string ContainerName, string ContainerId, string IPAddress, IDictionary<ushort, ushort> Ports)> RunContainer(DockerClient docker, string imageName, string tag, string containerName, params string[] env)
        {
            Console.WriteLine($"Running {imageName}");

            // Get the list of all the containers run on the particular docker instance.
            var containers = await docker.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true
            });

            var startedContainer = containers.FirstOrDefault(c => c.Names.Contains($"/{containerName}"));

            if (startedContainer != null)
            {
                Console.WriteLine($"Get rid of existing container {containerName}");

                // Remove container with the same name.
                await docker.Containers.RemoveContainerAsync(startedContainer.ID, new ContainerRemoveParameters { Force = true });
            }

            // Check if the image is already pulled.
            var images = await docker.Images.ListImagesAsync(new ImagesListParameters
            {
                All = true,
                MatchName = $"{imageName}:{tag}"
            });

            if (!images.Any())
            {
                Console.WriteLine($"Pulling the image {imageName}");

                // Pull the image.
                await docker.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageName,
                        Tag = tag
                    }, null, new Progress<JSONMessage>(m => Console.WriteLine($"Pulling image {imageName}:{tag}:\n{m.ProgressMessage}")));
            }

            Console.WriteLine($"Creating container {containerName}");

            // Now create the container from the image.
            var container = await docker.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Name = containerName,
                    Image = $"{imageName}:{tag}",
                    AttachStdout = true,
                    Env = env,
                    Hostname = containerName,
                    Domainname = containerName,
                    HostConfig = new HostConfig
                    {
                        PublishAllPorts = true,
                    }
                }, CancellationToken.None);

            Console.WriteLine($"Runing container {containerName}");

            // Container is not started automatically. Start it.
            var result = await docker.Containers.StartContainerAsync(container.ID, new ContainerStartParameters
            {
            });

            Console.WriteLine($"Container {containerName} run - {result}");

            // Get containers list again to check the containers status.
            containers = await docker.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true,
            });

            startedContainer = containers.FirstOrDefault(c => c.Names.Contains($"/{containerName}"));

            Console.WriteLine($"Container state: {startedContainer.State}");
            Console.WriteLine($"Container status: {startedContainer.Status}");
            Console.WriteLine($"Container IPAddress: {startedContainer.NetworkSettings.Networks.FirstOrDefault().Key} - {startedContainer.NetworkSettings.Networks.FirstOrDefault().Value.IPAddress}");

            return (containerName, container.ID, startedContainer.NetworkSettings.Networks.FirstOrDefault().Value.IPAddress, startedContainer.Ports.ToDictionary(p => p.PrivatePort, p => p.PublicPort));
        }

        private static string GetDockerDefaultUrl()
        {
            var dockerHostVar = Environment.GetEnvironmentVariable("DOCKER_HOST");
            var defaultDockerUrl =
                !string.IsNullOrEmpty(dockerHostVar)
                ? dockerHostVar
                : !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "unix:///var/run/docker.sock"
                    : "npipe://./pipe/docker_engine";
            return defaultDockerUrl;
        }
    }
}
