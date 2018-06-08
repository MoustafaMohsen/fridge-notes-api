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
    [Route("api/Testing")]
    public class TestingController : Controller
    {
        private readonly AppDbContext _context;

        public TestingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Testing
        [HttpGet]
        public IEnumerable<Grocery> GetGrocery()
        {
            return _context.Grocery;
        }

        // GET: api/Testing/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrocery([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var grocery = await _context.Grocery.SingleOrDefaultAsync(m => m.Id == id);

            if (grocery == null)
            {
                return NotFound();
            }

            return Ok(grocery);
        }

        // PUT: api/Testing/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGrocery([FromRoute] int id, [FromBody] Grocery grocery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != grocery.Id)
            {
                return BadRequest();
            }

            _context.Entry(grocery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroceryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Testing
        [HttpPost]
        public async Task<IActionResult> PostGrocery([FromBody] Grocery grocery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Grocery.Add(grocery);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
        }

        // DELETE: api/Testing/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrocery([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var grocery = await _context.Grocery.SingleOrDefaultAsync(m => m.Id == id);
            if (grocery == null)
            {
                return NotFound();
            }

            _context.Grocery.Remove(grocery);
            await _context.SaveChangesAsync();

            return Ok(grocery);
        }

        private bool GroceryExists(int id)
        {
            return _context.Grocery.Any(e => e.Id == id);
        }
    }
}