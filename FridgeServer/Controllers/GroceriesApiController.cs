using CoreUserIdentity._UserIdentity;
using FridgeServer.Helpers;
using FridgeServer.Models.Dto;
using FridgeServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MLiberary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FridgeServer.Controllers
{

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

        [AuthTokenClient]
        [HttpGet("")]
        public async Task<IActionResult> GetGrocery()
        {
            var Id = GetTokenId();
            try
            {
                var groceries = await groceriesService.GetGrocery(Id);
                return Ok( ret(groceries, "loaded") );
            }
            catch (AppException ex)
            {
                return Ok(ree(ex.Message));
            }
        }

        [AuthTokenClient]
        [HttpGet("grocerybyid")]
        public async Task<IActionResult> GetGrocery([FromQuery(Name ="groceryid")] int GroceryId, [FromQuery(Name = "friend")] string friendId)
        {
            var Id = GetTokenId();
            if (friendId != Id && string.IsNullOrWhiteSpace(friendId))
            {
                string IdValidation = await userService.UserIdOrFriendId(Id, friendId);
                if (IdValidation == null)
                {
                    return Ok(ree("Grocery could not be found"));
                }
                else
                {
                    Id = IdValidation;
                }
            }
            if (M.isNullOr0(GroceryId))
            {
                return BadRequest(ree("No id was sent"));
            }
            try
            {
                var response = new ResponseDto() { statusText = "loaded", value = await groceriesService.GetGroceryById(GroceryId, Id) };
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        [AuthTokenClient]
        [HttpPost("request/{req}")]
        public async Task<IActionResult> UpdateStatus([FromBody] GroceryDto groceryDto, [FromRoute] string req)
        {
            var grocery = groceryDto.grocery;
            var givenid = groceryDto.userId;
            var Id = GetTokenId();
            if ( givenid != Id )
            {
                string IdValidation = await userService.UserIdOrFriendId(Id,  givenid);
                if (IdValidation==null)
                {
                    return Ok(  ree("Grocery could not be found")  );
                }
                else
                {
                    Id = IdValidation;
                }
            }
            //Validations
            if (grocery.name == "")
                return BadRequest( ree("No Name") );
            if (grocery == null)
                return NotFound();


            if (req == "add")
            {
                try
                {
                    var user =await groceriesService.AddGrocery(grocery, Id);
                    return Ok( ret(user, "added") );
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }
            }
            if (req == "needed")
            {
                try
                {
                    await groceriesService.neededGrocery(grocery, Id,true);
                    return Ok(objRes(null,"needed") );
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }
            }
            if (req == "bought")
            {
                try
                {
                    await groceriesService.boughtgrocery(grocery, Id,true);
                    return Ok(objRes(null,"bought"));
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }
            }
            if (req == "edit")
            {
                try
                {
                    await groceriesService.editgrocery(grocery, Id);
                    return Ok(objRes(null, "edited"));
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }
            }
            if (req == "remove")
            {
                try
                {
                    await groceriesService.removegrocery(grocery, Id);
                    return Ok(objRes(null, "remove"));
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }
            }
            if (req == "delete")
            {
                try
                {
                    await groceriesService.deletegrocery(grocery, Id);
                    return Ok(objRes(null, "delete"));
                }
                catch (AppException ex)
                {
                    return BadRequest(ree(ex.Message));
                }

            }
            if (req == "guess")
            {
                try
                {
                    var guessTime = groceriesService.guessgrocery(grocery);
                    return Ok(ret(guessTime,"guessed"));
                }
                catch (AppException ex)
                {
                    return BadRequest( ree(ex.Message) );
                }
            }
            if(req == "NameExists")
            {
                var status = await groceriesService.GroceryExistsName(grocery.name, Id);
                return Ok(ret(status, "done"));
            }

            return BadRequest( ree( $"request not found, request:{String.Format(req)}" ) );
        }

        [AuthTokenClient]
        [HttpPost("nameExists")]
        public async Task<IActionResult> NameExists([FromBody] ValueDto valueDto)
        {
            var Id = GetTokenId();
            var status = await groceriesService.GroceryExistsName(valueDto.value, Id);
            return Ok(ret(status,"done"));
        }

        #region Response Converter
        //response value
        public ResponseDto<T> ret<T>(T value, string statusText)
        {
            var response = new ResponseDto<T>
            {
                statusText = statusText,
                value = value
            };
            return response;
        }

        public ResponseDto objRes(object value, string statusText)
        {
            var response = new ResponseDto
            {
                statusText = statusText,
                value = value
            };
            return response;
        }

        //response error
        public ResponseDto ree(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                error = "error";
            }
            var response = new ResponseDto
            {
                errors = error
            };
            return response;
        }
        #endregion

        #region Get User and Claim
        public string GetTokenId()
        {
            var claims = HttpContext.User.Claims;
            var claim = FindClaimNameIdentifier(claims);
            if (claim != null)
            {
                return claim.Value;
            }
            return null;
        }
        public Claim FindClaim(IEnumerable<Claim> Eclaims, string ClaimType)
        {
            var claims = Eclaims.ToList();
            for (int i = 0; i < claims.Count; i++)
            {
                var claim = claims[i];
                if (claim.Type == ClaimType)
                {
                    return claim;
                }

            }
            return null;
        }
        public Claim FindClaimNameIdentifier(IEnumerable<Claim> Eclaims)
        {
            return FindClaim(Eclaims, ClaimTypes.NameIdentifier);

        }
        #endregion
    }//class

}//namespace

