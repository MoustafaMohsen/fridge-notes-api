using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FridgeServer.Helpers;
using FridgeServer.Models.Dto;
using FridgeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FridgeServer.Controllers
{
    [Route("api/[controller]")]
    public class VerificationController : Controller
    {
        #region Protected Members
        protected IUserService userService;
        #endregion
        #region Constructor
        public VerificationController(IUserService _userService)
        {
            userService = _userService;
        }
        #endregion

        // GET api/<controller>/5
        [HttpGet("verifyemail")]
        public async Task<IActionResult> EmailVerify([FromQuery(Name = "verCode")] string verCode, [FromQuery(Name = "Id")] string Id)
        {
            if (string.IsNullOrWhiteSpace(Id)|| string.IsNullOrWhiteSpace(verCode))
            {
                return BadRequest(ree("Bad Arguments"));
            }
            try
            {
                var result =  await userService.VerifyEmailAsync(Id, verCode);
                return Ok(ret(result,"email sent"));
            }
            catch (AppException ex)
            {

                return BadRequest(ree(ex.Message));
            }
        }

        [HttpGet("sendverifyemail")]
        public async Task<IActionResult> SendVerfication([FromQuery(Name = "Id")]string Id, [FromQuery(Name = "force")]bool force=false)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                return BadRequest(ree("Bad Arguments"));
            }

            try
            {
                var response = await userService.sendEmailVerfication(Id, force:force);
                return Ok(ret(response,"email sent"));
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
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
        public string GetHost()
        {
            return HttpContext.Request.Host.Value;
        }
        #endregion

    }
}
