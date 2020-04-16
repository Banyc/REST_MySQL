using REST_MySQL.Models;
using Microsoft.AspNetCore.Mvc;
using DotNetCoreReactAdmin.Controllers;
using Microsoft.EntityFrameworkCore;

namespace REST_MySQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ReactAdminController<Person>
    {
        public PeopleController(PersonContext context) : base(context)
        {
            _table = context.People;
        }
    }
}
