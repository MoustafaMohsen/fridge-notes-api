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
    // View All:---GET:   api/GroceriesApi
    // Details:----GET:   api/GroceriesApi/5
    // Add:--------POST:  api/GroceriesApi
    // Edit:-------PUT:   api/GroceriesApi/5
    // Delete:-----DELETE:api/GroceriesApi/5
    // ReAdd:------GET :  api/Grpceries/ReAdd/5  /1760
    // Bought:-----GET :  api/GroceriesApi/Bought/5
    // Basic:------GET:   api/GroceriesApi/Basic/5


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
        

        //============ Request Handlers ===========//

        // GET: api/GroceriesApi
        [HttpGet]
        public IEnumerable<Grocery> GetGrocery()
        {
            return db.Grocery.Include("MoreInformations");
        }

        // GET: api/GroceriesApi/5
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
        // GET: api/GroceriesApi/Basic/5
        [HttpGet("Basic/{id}")]
        public async Task<IActionResult> GetBasicGrocery([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var grocery = await db.Grocery.SingleOrDefaultAsync(m => m.Id == id);

            if (grocery == null)
            {
                return Content("Not Found");
            }

            return Ok(grocery);
        }

        // GET: api/GroceriesApi/name/milk
        [HttpGet("name/{name}")]
        public bool? GetBasicGrocery([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            return GroceryExistsName(name);
        }

        /*

        */


        //====================================IMPORTANT REVERSE TIMEOUT LOGIC =================//timeout when grocery = 0 ,timeout when have/bought && basic = value 

        // PUT: api/GroceriesApi/5
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

        [Route("guess/{id}")]
        [HttpGet]
        public async Task<IActionResult> GuesTimeout(int id)
        {
            var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);

            if (MoreisNullOrEmpty(grocery?.MoreInformations))
            {
                return Content("Null Or Empty");
            }
            return Content( ""+HandleTimeout(grocery, 0) ) ;
        }

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
                //Delete
                var Deletegrocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == grocery.Id);

                if (Deletegrocery.MoreInformations.Count <= 1){return Content("Item has to be Deleted");}

                /*
                db.Grocery.Remove(Deletegrocery);
                try{await db.SaveChangesAsync();}
                catch (DbUpdateConcurrencyException){throw;}


                List<MoreInformations> HoldinformationArray=new List<MoreInformations>{};

                foreach (var item in grocery.MoreInformations)
                {
                    HoldinformationArray.Add(item);
                }

                grocery.Id = 0;
                grocery.MoreInformations = HoldinformationArray;       
                
                //Post
                db.Grocery.Add(grocery);
                await db.SaveChangesAsync();
                */
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


            if (req=="guess")
            {
                if (MoreisNullOrEmpty(grocery?.MoreInformations))
                {
                    return Content("Null Or Empty");
                }
                return Content("" + HandleTimeout(grocery, 0));
            }

            return Content(String.Format("No valide req was found, req={0} ,valid req are 'edit'  'bought'  'needed'  'add'", req));

        }






        //============ Helper Methods ===========//
        private bool GroceryExistsId(int id)
        {
            return db.Grocery.Any(e => e.Id == id);
        }
        private bool GroceryExistsName(string name)
        {
            return db.Grocery.Any(e => e.Name == name);
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
        private List<MoreInformations> Updatedate(List<MoreInformations> more)
        {
            int HoldNo = (int)more.Last().No;
            string HoldtypeOfNo = more.Last().typeOfNo;
            var now = (long)Alarm.DateTimeToUnixTime(DateTime.Now);
            long HoldLastedate = (long)more.Last().Date;
            MoreInformations listUpdate = new MoreInformations()
            {
                Bought = more.Last().Bought,
                Date = now,
                typeOfNo = HoldtypeOfNo,
                No = HoldNo,
                LifeTime = 0
            };

            return replaceLast(more, listUpdate);
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
        [Route("Test")]
        [HttpGet]
        public List<Grocery> GetGr()
        { 
             return FinalTest.List;
        }

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

    }//class

}//namespace



/***


//=====================================================   OLD CODE   ==================================================================//


/*
// POST: api/GroceriesApi
[HttpPost]
public async Task<IActionResult> PostGrocery([FromBody] Grocery grocery)
{
    //PostValidation
    var validate = validatePost(grocery);
    if (validate != passedValidation ){
        return Content(validate);
    }
    if (validate=="no timeout")
    {
        grocery.Timeout= HandleTimeout(grocery, grocery.Timeout);
    }

    //New Post Handle
    grocery = NewPostNullVAlidationAndAutoAssienments(grocery);

    //add logic
    db.Grocery.Add(grocery);
    await db.SaveChangesAsync();

    return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
}






// GET : api/Grpceries/Rebuy/5  /1760
[Route("Needed/{id}/{basic}")]
//[Route("Needed/{id}/{timeout}")]//Needed =>bought from True to false
[HttpGet]
public async Task<IActionResult> Needed([FromRoute]int id, [FromRoute]bool basic=true, [FromRoute]long timeout = 0)
{ 
//ReAdd logic
if (!ModelState.IsValid){ return BadRequest(ModelState); }//validate
var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);
if (grocery == null){  return Content("Id Not found"); }//validate


if ( grocery.MoreInformations?.Last().Bought==false){ return Content("Bought=false"); }//validate

// prepare logic
// grocery.Timeout = HandleTimeout(grocery,timeout);
var more = UpdateInformationsList(grocery.MoreInformations, false);
grocery.MoreInformations.Add(more);
grocery.basic = basic;
//edit logic
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
return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
}




// DELETE: api/GroceriesApi/5
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteGrocery([FromRoute] int id)
{
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}

var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);
if (grocery == null)
{
    return NotFound();
}

db.Grocery.Remove(grocery);
await db.SaveChangesAsync();

return Ok(grocery);
}




// GET :api/GroceriesApi/Bought/5
[Route("Bought/{id}")]
[HttpGet]
public async Task<IActionResult> Bought([FromRoute]int id)
{
//bought logic
if (!ModelState.IsValid) { return BadRequest(ModelState);}//validate
var grocery = await db.Grocery.Include("MoreInformations").SingleOrDefaultAsync(m => m.Id == id);
if (grocery == null){return Content("Id Not found"); }//validate

//Validation
if (grocery.MoreInformations.Count==0)//validate and add if failed
{
    //DELETE LATER
    var Holder = Alarm.DateTimeToUnixTime(  DateTime.Now.AddDays(3)  );
    var temp = new MoreInformations()
    {
        Bought = false,
        Date = (long)Holder,
    };
    grocery.MoreInformations.Add(temp);
}
if (grocery.MoreInformations?.Last().Bought == true) { return Content("Bought=true"); }
//Validation


//prepare logic
var more = UpdateInformationsList(grocery.MoreInformations, true);
grocery.MoreInformations.Add(more);


//edit logic
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

return CreatedAtAction("GetGrocery", new { id = grocery.Id }, grocery);
}






//===============old Helpers

private Grocery UpdateLifeTime(Grocery grocery,bool Bought =false)
{

var more = UpdateInformationsList(grocery.MoreInformations , Bought);
grocery.MoreInformations.Add(more);

return grocery;
}


private Grocery NewPostNullVAlidationAndAutoAssienments(Grocery grocery)
{
    //nullValidation
    bool BoughtHolder;
    long HoldLifetime;
    int HoldNo;
    string HoldtypeOfNo;
    if (
        !MoreisNullOrEmpty(grocery.MoreInformations)

        )//MoreInformations Has a value
    {


        HoldLifetime = isNull(grocery.MoreInformations.Last().LifeTime) ? 0 :
            (long)grocery.MoreInformations.Last().LifeTime;

        BoughtHolder = grocery.MoreInformations.Last().Bought == null ?
            false : grocery.MoreInformations.Last().Bought;

        HoldNo = grocery.MoreInformations.Last().No == null ? 1 :
            (int)grocery.MoreInformations.Last().No;

        HoldtypeOfNo = grocery.MoreInformations.Last().typeOfNo == null ? "" :
            grocery.MoreInformations.Last().typeOfNo;
        grocery.MoreInformations.RemoveAt(grocery.MoreInformations.Count - 1);

    }
    else//MoreInformations is Null
    {
        BoughtHolder = false;
        HoldLifetime = 0;
        HoldNo = 1;
        HoldtypeOfNo = "";
    }

    MoreInformations list = new MoreInformations()
    {
        Bought = BoughtHolder,
        Date = (long)Alarm.DateTimeToUnixTime(DateTime.Now),
        LifeTime = HoldLifetime,
        No = HoldNo,
        typeOfNo = HoldtypeOfNo
    };


    grocery.MoreInformations.Add(list);
    return grocery;

}


// GET: api/GroceriesApi/name
[HttpGet("name/{name}")]
public async Task<IActionResult> GetGrocery([FromRoute] string name)
{
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
if (GroceryExistsName(name)) return Content("Name does not exist");
var grocery =  db.Grocery.Where(e => e.Name == name);

if (grocery == null)
{
    return Content("Not Found");
}

return Ok(grocery);
}

**/


/// <summary>
/// Grocery non nullabls
/// MoreInformations[ No=1 ,typeOfNo="" , bought =false ,  ] lifetime is a server property only
/// Name,basic=false
/// If no timeout and basic is true then let the server handls it else just send it as difference not the future
/// </summary>
/// forntend is the orcestrator and the one who determens the state of grocery before request
/// Add,Edit,Needed,Bought,remove all are a single Post method and to determine the state send in parameter[needed,bought,edit,add]
///  
/// server dependencies:-
/// -Guessing timeout
/// -formating timeout to future
/// -setting Date and lifetime
/// -set lifetime only at bought invoks , set timeout only at needed invoks
/// -timeout == 0 then it's not basic
/// 
/// frontend dependencies
/// -forming a request suitable for add,edit,bought,needed
///     -bought =>grocery as it is just add moreinformation at the end of the array containing
///       -[No=last ,typeOfNo=last , bought =true ] To URL/bought
///       
///     -needed =>grocery as [Name=same,basic=input,timout=input(ifbasic)  ] and add moreinformation at the end of the array containing
///       -[No=lastOrinput ,typeOfNo=lastOrinput , bought =false ] To URL/needed
/// 
///     -edit =>grocery as [Name=Input,basic=input,timout=input(ifbasic&needed show input else last or 0 if basic=false) ] and  
///       Change moreinformation at the end of the array to [No=input ,typeOfNo=input , bought=holdlast ] 
///       To URL/needed
///       
///     -remove => grocery as if moreinformation.count > 1 then remove last moreinformation and send edit
///         else send DELETE request
///     
///     -add => grocery as [Name=Input,basic=inputOrfalse ,timout=input(if basic show input else 0 if basic=false) ] and  
///       Change moreinformation at the end of the array to [No=inputOr1 ,typeOfNo=inputOr"" , bought=inputOrfalse ] 
///       To URL/add
///  
/// -DELETE =>send Delete request conatining id
/// 
/// 
/// <param name="grocery"></param>
/// 
/// <returns></returns>
///     /// <summary>
/// -To add normal item
///     1-see if there any items with the same name that already exists , if so send a snakebar containing error massage,else
///     2-send a post to ADD containing only the name
/// -To  add basic 
///     1-see if there any items with the same name that already exists , if so send a snakebar containing error massage,else
///     2-validate requierd data(timeout,basic,name)then send a post to ADD containing the the grocery 
/// -Bought item
///     1-check server if that bought=false,else send snakebar error
///     2-send a bought request
/// -ReAdd
///     1-check that the item already exists, else send snakebar
///     2-get the item timeout and Details and bind it to the inputs
///     3-send ReAdd request
/// -Edit
///     1-send edit request --need some work to make it possipole to edit moreinformation
/// -to Delete Item
///     1-Send a Delete request
/// 
/// </summary>