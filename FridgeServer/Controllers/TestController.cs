using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Data;
using FridgeServer.Models;

namespace FridgeServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Test")]
    public class TestController : Controller
    {
      //  private readonly AppDbContext _context;
      
        public TestController(/*AppDbContext context*/)
        {
          //  _context = context;
        }

        // GET: api/Test
        [HttpGet]
        public IEnumerable<string> GetGrocery()
        {
            return new string[] { "value1", "value2" };
            
        }

        // GET: api/Test/5
        [HttpGet("{id}")]
        public  IActionResult GetGrocery([FromRoute] int id)
        {
            return Ok(id);
        }

        // PUT: api/Test/5
        [HttpPut("{id}")]
        public  IActionResult PutGrocery([FromRoute] int id, [FromBody] Grocery grocery)
        {

            return Json(grocery);
        }

        // POST: api/Test
        [HttpPost]
        public IActionResult PostGrocery([FromBody] Grocery grocery)
        {
            return Json(grocery);
        }

        // DELETE: api/Test/5
        [HttpDelete("{id}")]
        public  IActionResult DeleteGrocery([FromRoute] int id)
        {
            return Ok("Deleted ID Is :"+id);
        }

    }
}