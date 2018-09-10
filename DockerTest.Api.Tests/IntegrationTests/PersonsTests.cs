using DockerTest.Api.Database;
using DockerTest.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DockerTest.Api.Tests.IntegrationTests
{
    [Collection("IntegrationCollection")]
    public class PersonsTests : TestBase
    {
        public PersonsTests(IntegrationFixture integrationFixture)
            : base(integrationFixture)
        {
        }

        [Fact]
        public async Task CreatePerson_WhenPostValidData_ShouldReturnNoContent()
        {
            // Arrange
            var person = new PersonContract { FirstName = "Name4", LastName = "LastName4", Age = 35 };

            // Act
            var postResult = await Client.PostAsync("api/persons",
                new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Created, postResult.StatusCode);

            // Make sure that returned data is valid.
            var createdPerson = await postResult.Content.ReadAsAsync<PersonContract>();
            AssertSamePerson(person, createdPerson);

            // Now get the person from API to make sure it was created.
            var getResult = await Client.GetAsync($"api/persons/{createdPerson.Id}");
            Assert.True(getResult.IsSuccessStatusCode);

            var receivedPerson = await getResult.Content.ReadAsAsync<PersonContract>();

            // Make sure that we received the same data as sent.
            AssertSamePerson(person, receivedPerson);
        }

        private static void AssertSamePerson(PersonContract p1, PersonContract p2)
        {
            Assert.Equal(p1.FirstName, p2.FirstName);
            Assert.Equal(p1.LastName, p2.LastName);
            Assert.Equal(p1.Age, p2.Age);
        }

        public static async Task ArrangeData(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService<PersonsContext>();

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(async () =>
            {
                await context.Database.ExecuteSqlCommandAsync("SET IDENTITY_INSERT Persons ON");

                context.AddRange(
                    new Person { Id = 10, FirstName = "Name1", LastName = "LastName1", Age = 20 },
                    new Person { Id = 11, FirstName = "Name2", LastName = "LastName2", Age = 25 },
                    new Person { Id = 12, FirstName = "Name3", LastName = "LastName3", Age = 30 });

                await context.SaveChangesAsync();

                await context.Database.ExecuteSqlCommandAsync("SET IDENTITY_INSERT Persons OFF");
            }, () => Task.FromResult(true));
        }
    }
}
