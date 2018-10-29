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
using Microsoft.AspNetCore.Authorization;
using FridgeServer.Helpers;
using FridgeServer.Models.Dto;

namespace FridgeServer.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class GroceriesApiController : Controller
    {
        private readonly IGroceriesService groceriesService;
        private IUserService userService;

        public GroceriesApiController(IGroceriesService _IGroceriesService, IUserService _userService)
        {
            groceriesService = _IGroceriesService;
            userService = _userService;
        }

        [HttpGet("")]
        public IActionResult GetGrocery()
        {
            var id = int.Parse(HttpContext.User.Identity.Name);
            try
            {
                var response = new ResponseDto() { statusText = "loaded", value = groceriesService.GetGrocery(id) };
                return Ok(response);
            }
            catch (AppException ex)
            {
                var response = BadRequest(ex);
                return response;
            }
        }

        [HttpGet("{GroceryId}")]
        public IActionResult GetGrocery([FromRoute] int GroceryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userid = int.Parse(HttpContext.User.Identity.Name);
            try
            {
                return Ok( groceriesService.GetGroceryById(GroceryId, userid) );
            }
            catch (AppException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("request/{req}")]
        public IActionResult UpdateStatus([FromBody] GroceryDto groceryDto, [FromRoute] string req)
        {
            var grocery = groceryDto.grocery;
            var givenid = groceryDto.userId;
            var id = int.Parse(HttpContext.User.Identity.Name);
            int? IdValidation;
            if (givenid != id)
            {
                IdValidation = userService.IdValidation(userId: id, GevenId: givenid);
                if (IdValidation==null)
                {
                    return BadRequest("Bad Grocery");
                }
                else
                {
                    id = (int)IdValidation;
                }
            }
            //Validations
            if (!ModelState.IsValid) { return BadRequest(ModelState); }//validate
            if (grocery.name == "") { return BadRequest("No Name"); }//validate
            if (grocery == null) { return NotFound(); }//validate


            if (req == "add")
            {
                try
                {
                    groceriesService.AddGrocery(grocery, id);
                    var response = new ResponseDto() {
                        statusText="added",
                        value=null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if (req == "needed")
            {

                try
                {
                    groceriesService.neededGrocery(grocery, id);
                    var response = new ResponseDto()
                    {
                        statusText = "needed",
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if (req == "bought")
            {
                try
                {
                    groceriesService.boughtgrocery(grocery, id);
                    var response = new ResponseDto()
                    {
                        statusText = "bought",
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if (req == "edit")
            {
                try
                {
                    groceriesService.editgrocery(grocery, id);
                    var response = new ResponseDto()
                    {
                        statusText = "edited",
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if (req == "remove")
            {
                try
                {
                    groceriesService.removegrocery(grocery, id);
                    var response = new ResponseDto()
                    {
                        statusText = "remove",
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if (req == "delete")
            {
                try
                {
                    groceriesService.deletegrocery(grocery, id);
                    var response = new ResponseDto()
                    {
                        statusText = "delete",
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }

            }
            if (req == "guess")
            {
                try
                {
                    var response = new ResponseDto()
                    {
                        statusText = groceriesService.guessgrocery(grocery),
                        value = null
                    };
                    return Ok(response);
                }
                catch (AppException ex)
                {
                    return BadRequest(ex);
                }
            }
            if(req == "NameExists")
            {
                if (!ModelState.IsValid)
                {
                    return null;
                }
                var response = new ResponseDto()
                {
                    statusText = "",
                    value = groceriesService.GroceryExistsName(grocery.name, id)
                };
                return Ok(response);
            }

            return BadRequest( String.Format(req) );
        }

        [HttpPost("nameExists")]
        public IActionResult NameExists([FromBody] ValueDto valueDto)
        {
            var id = int.Parse(HttpContext.User.Identity.Name);
            if (!ModelState.IsValid)
            {
                var response1 = new ResponseDto()
                {
                    statusText = "BadRequest",
                    value = false
                };
                return Ok(response1);
            }
            var response = new ResponseDto()
            {
                statusText = "",
                value = groceriesService.GroceryExistsName(valueDto.value, id)
            };
            return Ok(response);
        }

    }//class

}//namespace

