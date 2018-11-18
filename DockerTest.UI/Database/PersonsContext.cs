using Microsoft.EntityFrameworkCore;

namespace DockerTest.UI.Database
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public PersonsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasData(
                new Person { Id = 1, Name = "Aliaksei Harshkalep1", Age = 21, Address = "Belarus, Gomel" },
                new Person { Id = 2, Name = "Aliaksei Harshkalep2", Age = 22, Address = "Belarus, Gomel" },
                new Person { Id = 3, Name = "Aliaksei Harshkalep3", Age = 23, Address = "Belarus, Gomel" },
                new Person { Id = 4, Name = "Aliaksei Harshkalep4", Age = 24, Address = "Belarus, Gomel" },
                new Person { Id = 5, Name = "Aliaksei Harshkalep5", Age = 25, Address = "Belarus, Gomel" },
                new Person { Id = 6, Name = "Aliaksei Harshkalep6", Age = 26, Address = "Belarus, Gomel" },
                new Person { Id = 7, Name = "Aliaksei Harshkalep7", Age = 27, Address = "Belarus, Gomel" },
                new Person { Id = 8, Name = "Aliaksei Harshkalep8", Age = 28, Address = "Belarus, Gomel" },
                new Person { Id = 9, Name = "Aliaksei Harshkalep9", Age = 29, Address = "Belarus, Gomel" },
                new Person { Id = 10, Name = "Aliaksei Harshkalep10", Age = 30, Address = "Belarus, Gomel" },
                new Person { Id = 11, Name = "Aliaksei Harshkalep11", Age = 31, Address = "Belarus, Gomel" },
                new Person { Id = 12, Name = "Aliaksei Harshkalep12", Age = 32, Address = "Belarus, Gomel" },
                new Person { Id = 13, Name = "Aliaksei Harshkalep13", Age = 33, Address = "Belarus, Gomel" },
                new Person { Id = 14, Name = "Aliaksei Harshkalep14", Age = 34, Address = "Belarus, Gomel" },
                new Person { Id = 15, Name = "Aliaksei Harshkalep15", Age = 35, Address = "Belarus, Gomel" });
        }
    }
}
