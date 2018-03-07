using DockerTest.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DockerTest.Api.Database
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public PersonsContext(DbContextOptions<PersonsContext> options)
            : base(options)
        {
        }
    }
}
