using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Data;
using FridgeServer.Models;
using FridgeServer.Services;
using Microsoft.AspNetCore.Cors;

namespace FridgeServer.Controllers
{

    [Produces("application/json")]
    [Route("api/GroceriesApi")]
    public class GroceriesApiController : Controller
    {
        private readonly AppDbContext db;
        private  GuessTimeout guessTimeout;
        private const string passedValidation = "passed Validation";
        public GroceriesApiController(AppDbContext context,GuessTimeout _guessTimeout)
        {
            db = context;
            guessTimeout = _guessTimeout;
        }


        /// <summary>
        /// BASE : "api/GroceriesApi"
        /// 
        /// ===== Gets
        /// All groceries  :""
        /// By id  :"{id}"
        ///     
        /// ===== Updates
        /// Edit   : "putedit/{id}"
        /// Update : "request/{req}" bought,needed,remove,delete
        /// 
        /// ===== Services
        /// Check Name    : "name/{name}"
        /// Guess Timeout : "guess/{id}"
        /// </summary>


        //============ Request Handlers ===========//

        //===== Gets
        // GET
        [HttpGet]
        public IEnumerable<Grocery> GetGrocery()
        {
            return db.Grocery.Include("MoreInformations");
        }

        // GET
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrocery([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);

            if (grocery == null)
            {
                return Content("Not Found");
            }

            return Ok(grocery);
        }

        //===== Updates
        // PUT
        [HttpPut("putedit/{id}")]
        public async Task<IActionResult> PutGrocery([FromRoute] int id, [FromBody] Grocery grocery)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            if (id != grocery.Id) { return BadRequest(); }

            db.Entry(grocery).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroceryExistsId(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok("Ok"); ;
        }

        // POST 
        [Route("request/{req}")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] Grocery grocery, [FromRoute] string req)
        {
            //Validations
            if (!ModelState.IsValid) { return BadRequest(ModelState); }//validate
            if (grocery.Name == "") { return Content("No Name"); }//validate
            if (grocery == null) { return NotFound(); }//validate


            if (req == "add")
            {
                //--------------add logic-------------//
                //PostValidation
                if (GroceryExistsName(grocery.Name)) { return Content("name already exists"); }//validate

                grocery.MoreInformations = UpdateInformationsAdd(grocery.MoreInformations);

                db.Grocery.Add(grocery);
                await db.SaveChangesAsync();

                return Ok("Added");
            }
            //--------------add logic-------------//   


            //--------------needed logic-------------//
            if (req == "needed")
            {

                grocery.Timeout = 0;//override the timeout

                var editgrocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == grocery.Id);
                if (editgrocery == null) { return Content("Id Not found"); }//validate

                editgrocery.Timeout = 0;

                // editgrocery.MoreInformations = UpdateLifetimeanddate(editgrocery.MoreInformations,2);
                var more = UpdateInformationsList(editgrocery.MoreInformations, false);
                editgrocery.MoreInformations.Add(more);

                /*
                db.Entry(grocery.MoreInformations).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }*/

                db.Entry(editgrocery).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return Ok("Neededed ¯\\_(ツ)_/¯");
                //  return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
            }
            //--------------needed logic-------------//

            //--------------bought logic-------------//
            if (req == "bought")
            {
                var editgrocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == grocery.Id);
                if (editgrocery == null) { return Content("Id Not found"); }//validate

                editgrocery.Timeout = HandleTimeout(editgrocery, grocery.Timeout);


                //  grocery.MoreInformations = UpdateLifetimeanddate(grocery.MoreInformations,2);

                var more = UpdateInformationsList(editgrocery.MoreInformations, true);
                editgrocery.MoreInformations.Add(more);


                // grocery.MoreInformations[grocery.MoreInformations.Count - 1].MoreInformationsId = null;

                db.Entry(editgrocery).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return Ok("bought");
                // return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
            }
            //--------------bought logic-------------//


            //--------------edit logic-------------//
            if (req == "edit")
            {
                if (!ModelState.IsValid) { return BadRequest(ModelState); }


                db.Entry(grocery).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroceryExistsId(grocery.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok("edited");
            }
            //--------------edit logic-------------//



            //--------------remove logic-------------//
            if (req == "remove")
            {
                //remove
                var Deletegrocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == grocery.Id);
                if (Deletegrocery.MoreInformations.Count <= 1) { return Content("Item has to be Deleted"); }
                var holdInfo = new List<MoreInformations>();
                Deletegrocery.MoreInformations.RemoveAt(grocery.MoreInformations.Count -1);

                //edit
                db.Entry(Deletegrocery).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                        throw;
                }
                return Ok("Ok"); 
                
                // return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
            }
            //--------------remove logic-------------//


            //--------------delete logic-------------//
            if (req == "delete")
            {
                var Deletegrocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == grocery.Id);
                if (grocery == null)
                {
                    return NotFound();
                }

                db.Grocery.Remove(Deletegrocery);
                try
                {
                    await db.SaveChangesAsync();
                    return Ok("Deleted");
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

            }
            //--------------delete logic-------------//


            if (req == "guess")
            {
                if (MoreisNullOrEmpty(grocery?.MoreInformations))
                {
                    return Content("Null Or Empty");
                }
                return Content("" + HandleTimeout(grocery, 0));
            }

            return Content(String.Format("No valide req was found, req={0} ,valid req are 'edit'  'bought'  'needed'  'add'", req));

        }

        //===== Services
        // GET
        [HttpGet("name/{name}")]
        public bool? GetBasicGrocery([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }
            return GroceryExistsName(name);
        }

        [Route("guess/{id}")]
        [HttpGet]
        public async Task<IActionResult> GuesTimeout(int id)
        {
            var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);

            if (MoreisNullOrEmpty(grocery?.MoreInformations))
            {
                return Content("Null Or Empty");
            }
            return Content("" + HandleTimeout(grocery, 0));
        }


        //============ Helper Methods ===========//
        private bool GroceryExistsId(int id)
        {
            return db.Grocery.Any(e => e.Id == id);
        }
        private bool GroceryExistsName(string name)
        {
            return db.Grocery.Any(e => e.Name.ToLower() == name.ToLower() );
        }
        private long HandleTimeout(Grocery grocery, long? timeout)
        {
            if ((timeout <= 0) || timeout == null)//if no timeout or invalid
            {
                grocery.Timeout = guessTimeout.GuessByInformation(grocery.MoreInformations);
                return (long)grocery.Timeout;
            }
            else
            {
                return (long)timeout;//+ (long)Alarm.DateTimeToUnixTime(DateTime.Now); ;
            }
        }
        private List<MoreInformations> UpdateLifetimeanddate(List<MoreInformations> more,int lastDateBy=1)
        {
            int HoldNo = (int)more.Last().No;
            string HoldtypeOfNo = more.Last().typeOfNo;
            var now = (long)Alarm.DateTimeToUnixTime(DateTime.Now);
            long HoldLastDate =(long) more[more.Count - lastDateBy].Date;

            MoreInformations listUpdate = new MoreInformations()
            {
                Bought = more.Last().Bought,
                Date = now,
                typeOfNo = HoldtypeOfNo,
                No = HoldNo,
                LifeTime = now - HoldLastDate
            };

            return replaceLast(more, listUpdate);
        }
        private List<MoreInformations> replaceLast(List<MoreInformations> more, MoreInformations listUpdate)
        {
            try
            {
                more[more.Count - 1] = listUpdate;

                return more;
            }
            catch (Exception) { throw; }

        }
        private MoreInformations UpdateInformationsList(List<MoreInformations> more , bool bought)
        {
            long HoldLastDate = isNull(more.Last().Date) ?
                (long)Alarm.DateTimeToUnixTime(DateTime.Now) //if it Null
                : (long)more.Last().Date;//if Date has Value

            int HoldNo = isNull(more.Last().No) ?
                1 //if it Null
                : (int)more.Last().No;

            string HoldtypeOfNo = isNull(more.Last().typeOfNo) ?
                "" //if it Null
                : more.Last().typeOfNo;

            var now = (long)Alarm.DateTimeToUnixTime(DateTime.Now);

            MoreInformations listUpdate = new MoreInformations()
            {
                Bought = bought,
                Date = now,
                LifeTime = now - HoldLastDate,
                typeOfNo = HoldtypeOfNo,
                No = HoldNo
            };

            return listUpdate;
        }
        private string validatePost(Grocery grocery)
        {
            if (!ModelState.IsValid) { return "Json state is bad"; }//validate

            //check for basic and timeout errors
            if ((grocery.basic && (grocery.Timeout == null || grocery.Timeout <= 0))
               || (!grocery.basic && (grocery.Timeout != null || grocery.Timeout >= 0 || grocery.Timeout < 0))
               )
            {
                return "no timeout";
            }

            return passedValidation;
        }
        private List<MoreInformations> UpdateInformationsAdd(List<MoreInformations> more)
        {
            int HoldNo = (int)more.Last().No;
            string HoldtypeOfNo = more.Last().typeOfNo;
            var now = (long)Alarm.DateTimeToUnixTime(DateTime.Now);

            MoreInformations listUpdate = new MoreInformations()
            {
                Bought = more.Last().Bought,
                Date = now,
                typeOfNo = HoldtypeOfNo,
                No = HoldNo,
                LifeTime = 0
            };

            return replaceLast(more,listUpdate); 
        }
        private bool MoreisNullOrEmpty(List<MoreInformations> Object)
        {
            if (Object == null)
            {

                return true;
            }
            else
            {
                if (Object.Count == 0)
                {
                    return true;
                }

                return false;
            }
        }
        private bool isNull<T>(T Object)
        {
            if (Object == null )
            {
                return true;
            }
            return false;
        }




        //==================================================TESTs================================================//
        /*

        [Route("TestPost/{id}/")]
        [Route("TestPost/{id}/{timeout}")]
        [HttpGet]
        public  IActionResult test(int id,long timeout=0)
        {
           
            return Content(passedValidation);
        }

        //TEst Rout
        [Route("Test2")]
        [HttpGet]
        public string Get2()
        {
            return db.Grocery.ToList().ToString();
        }
        */

    }//class

}//namespace

