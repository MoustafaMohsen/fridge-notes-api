using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FridgeServer.Models;
using FridgeServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FridgeServer.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class GListController : Controller
    {
        private readonly AppDbContext db;
        public GListController(AppDbContext context) {
   
            db = context;

            saveFromDbToLocal();
            
        }

        //=========== Helper Methods =========//
        //save to local variabl
        private List<Grocery> GroceryList = new List<Grocery>();
        private void saveFromDbToLocal()
        {
            GroceryList = db.Grocery.ToList();
        }

        //Check if a data exists
        private bool GroceryExists(int id)
        {
            return db.Grocery.Any(e => e.Id == id);
        }





        //=========== Request Handllers =========//

        //get
        [HttpGet]
        public IEnumerable<Grocery> Get()
        {
            return  GroceryList;
        }

        //Post
        [HttpPost]
        public ActionResult Create([FromBody]Grocery item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            db.Grocery.Add(item);
            db.SaveChanges();

            saveFromDbToLocal();

            return Json(GroceryList);
        }





    }

    //Old Code
    /*
     
        //MOCK
        List<Grocery> ListMock =new  List<Grocery>() {
            new Grocery{Name="ketchap"},
            new Grocery{ Name="milk"},
            new Grocery{Name="cheese"},
            new Grocery{Name="oil"},
            new Grocery{Name="sugar",basic=true},
            new Grocery{Name="salt"}
        };


        //run only once in lifetime of build in constructor
        if (db.Grocery.Count() == 0)
        {
            for (int i = 0; i < ListMock.Count ; i++)
            {
                db.Grocery.Add(ListMock[i]);
            }
            db.SaveChanges();
        }


        // POST: glist/Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return Redirect("google.com");
            var grocery = await db.Grocery.SingleOrDefaultAsync(m => m.Id == id);
            db.Grocery.Remove(grocery);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Get));
        }


        
     */
}