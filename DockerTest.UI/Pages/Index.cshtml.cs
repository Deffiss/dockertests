using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DockerTest.UI.Database;

namespace DockerTest.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DockerTest.UI.Database.PersonsContext _context;

        public IndexModel(DockerTest.UI.Database.PersonsContext context)
        {
            _context = context;
        }

        public IList<Person> Person { get;set; }

        public async Task OnGetAsync()
        {
            Person = await _context.Persons.ToListAsync();
        }
    }
}
