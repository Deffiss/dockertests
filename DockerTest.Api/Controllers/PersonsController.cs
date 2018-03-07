using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DockerTest.Api.Database;
using DockerTest.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DockerTest.Api.Controllers
{
    [Route("api/[controller]")]
    public class PersonsController : Controller
    {
        private readonly PersonsContext _context;

        public PersonsController(PersonsContext context)
        {
            _context = context;
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person[]))]
        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _context.Persons.ToArrayAsync());

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person))]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) => Ok(await _context.Persons.FindAsync(id));

        [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(Person))]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Person person)
        {
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { person.Id }, person);
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent, Type = typeof(void))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var person = new Person { Id = id };
            _context.Attach(person);
            _context.Entry(person).State = EntityState.Deleted;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
