using AutoMapper;
using CoreUserIdentity._UserIdentity;
using CoreUserIdentity.Models;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using FridgeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLiberary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FridgeServer.Controllers
{

    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private IUserService userService;
        private IMapper mapper;
        public UsersController(IUserService _userService, IMapper _mapper)
        {
            userService = _userService;
            mapper = _mapper;
        }

        [AuthTokenAny]
        [HttpGet("")]
        public async Task<IActionResult> GetUserId()
        {
            var CurrentUserId = GetTokenId();
            try
            {
                var dtoUser = await userService.GetById_IncludeRole(CurrentUserId);
                if (dtoUser == null)
                {
                    return BadRequest(ree("User doesn't Exsists"));
                }
                return Ok(ret(dtoUser, "Done"));
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        //Get User Id
        [AuthTokenAny]
        [HttpGet("GetUserId")]
        public async Task<IActionResult> GetUserId_1()
        {
            var CurrentUserId = GetTokenId();
            try
            {
                var dtoUser = await userService.GetUserById_include_roles_externalLogin(CurrentUserId);
                if (dtoUser == null)
                {
                    return BadRequest(ree("User doesn't Exsists"));
                }
                return Ok( ret(dtoUser,"Done") );
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        //Create a friend
        [AuthTokenClient]
        [HttpPost("addfriend")]
        public async Task<IActionResult> AddFreindToMe([FromBody]FriendRequestDto friendRequestDto)
        {
            friendRequestDto.userId = GetTokenId();
            try
            {
                UserFriend friend= await userService.AddFreindToMe(friendRequestDto);
                return Ok(ret(friend,"added"));
            }
            catch (AppException ex)
            {
                return BadRequest( ree(ex.Message) );
            }
        }

        //Delete a friend
        [AuthTokenClient]
        [HttpPost("deletefriendship")]
        public async Task<IActionResult> DeleteFriendship([FromBody]FriendRequestDto friendRequestDto)
        {
            friendRequestDto.userId = GetTokenId();
            try
            {
                var isDeleted = await userService.DeleteFriendship(friendRequestDto);
                return Ok( ret(isDeleted, isDeleted?"Deleted":"Error") );
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        //generateinvitation
        [AuthTokenClient]
        [HttpGet("generateinvitation")]
        public async Task<IActionResult> GenerateInv()
        {
            var CurrentUserId = GetTokenId();
            try
            {
                var invCode = await userService.GenerateUserInvitaionCodeAsync(CurrentUserId);
                return Ok(ret(invCode,"Generated"));
            }
            catch (AppException ex)
            {
                return BadRequest( ree(ex.Message) );
            }
            
        }

        //Edit User
        [AuthTokenAny]
        [HttpPut("editUser")]
        public async Task<IActionResult> UpdateUser(UserDto userDto,[FromQuery(Name ="force")]bool force=false)
        {
            var CurrentUserId = GetTokenId();
            try
            {
                if (force&&string.IsNullOrWhiteSpace(userDto.externalProvider))
                {
                    return BadRequest(ree("authentication problem, please try relogin"));
                }
                bool requirePassword = !force;
                var editeduser = await userService.Update(userDto, requirePassword);
                var applicationUser =await userService.GetUserById_include_roles_externalLogin(editeduser.Id);
                var editeduserDto = mapper.Map<UserDto>(applicationUser);
                editeduserDto.token = await userService.GenerateUserToken(editeduser, 7);
                return Ok(ret(editeduserDto,"Done"));
                
            }
            catch (AppException ex)
            {
                return BadRequest( ree(ex.Message) );
            }
            catch (Exception ex)
            {
                return BadRequest(ree(ex.Message));
            }

        }

        //Change password
        [AuthTokenAny]
        [HttpPut("changepassword")]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDto passwordDto, [FromQuery(Name = "force")]bool force = false)
        {

            var CurrentUserId = GetTokenId();
            if (CurrentUserId != passwordDto.id)
            {
                return BadRequest(ree("id Invalid"));
            }
            passwordDto.id = CurrentUserId;
            try
            {
                if (force && string.IsNullOrWhiteSpace(passwordDto.externalProvider))
                {
                    return BadRequest(ree("authentication problem, please try relogin"));
                }
                var editeduser = await userService.UpdatePasswordForceAsync(CurrentUserId,passwordDto.newpassword, passwordDto.externalProvider);
                var applicationUser = await userService.GetUserById_include_roles_externalLogin(editeduser.Id);
                var editeduserDto = mapper.Map<UserDto>(applicationUser);
                var statusText = "Done";
                return Ok(ret(editeduserDto,statusText));
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }

        }

        //Delete User
        [AuthTokenAny]
        [HttpPut("DeleteUser")]
        public async Task<IActionResult> DeleteUser(UserDto userDto)
        {
            var CurrentUserId = GetTokenId();
            try
            {
                var editeduser = await userService.Delete(CurrentUserId, userDto.password);
                return Ok( ret(editeduser, editeduser?"Deleted":"Failed") );
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        //Delete User
        [AuthTokenAny]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            var CurrentUserId = GetTokenId();
            try
            {
                var editeduser = await userService.DeleteNoPassword(CurrentUserId);
                return Ok(ret(editeduser, editeduser ? "Deleted" : "Failed"));
            }
            catch (AppException ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }


        // Anonumous
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> AuthenticatePost([FromBody]LoginUserDto userDto)
        {
            UserDto user =null;
            try
            {
                user = await userService.Login(userDto);
                return Ok( ret(user, "Login successful") );
            }
            catch (Exception ex)
            {
                return BadRequest( ree("Username or password is incorrect") );
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody]UserDto userDto)
        {
            try
            {
                var registerdUser = await userService.Create(userDto, userDto.password);
                return Ok( ret(registerdUser,"User created") );
            }
            catch (Exception ex)
            {
                return BadRequest( ree(ex.Message) );
            }
        }

        [AllowAnonymous]
        [HttpPost("external-facebook")]
        public async Task<IActionResult> Creata_Login_UserFacebook([FromQuery(Name = "code")]string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ree("Bad Parameters"));
            }
            try
            {
                var fridgeExternalResults = await userService.Facebook_Login_Register( code);
                if (!fridgeExternalResults.isSuccessful)
                {
                    return BadRequest(ree(fridgeExternalResults.errors));
                }
                return Ok(ret(fridgeExternalResults.User, "Logged in"));

            }
            catch (Exception ex)
            {
                return BadRequest(ree(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpPost("external-google")]
        public async Task<IActionResult> Creata_Login_UserGoogle([FromQuery(Name = "code")]string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ree("Bad Parameters"));
            }
            try
            {
                var fridgeExternalResults = await userService.Google_Login_Register(code);
                if (!fridgeExternalResults.isSuccessful)
                {
                    return BadRequest(ree(fridgeExternalResults.errors));
                }
                return Ok(ret(fridgeExternalResults.User, "Logged in"));

            }
            catch (Exception ex)
            {
                return BadRequest(ree("authentication failed"));
            }
        }

        //Dos User Exsits
        [AllowAnonymous]
        [HttpPost("UserExsits")]
        public async Task<IActionResult> DoesUserExsits([FromBody]UserDto userDto)
        {
            try
            {
                var state = await userService.IsUserNameExists(userDto.UserName);
                return Ok( ret(state,"done") );
            }
            catch (AppException ex)
            {
                return BadRequest( ree(ex.Message) );
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
    }//class
}
