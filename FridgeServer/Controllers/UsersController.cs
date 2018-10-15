using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using FridgeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FridgeServer.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private IUserService userService;
        private IMapper mapper;
        public UsersController(IUserService _userService, IMapper _mapper)
        {
            userService = _userService;
            mapper = _mapper;
        }
        
        //=======Admin Tools
        //Get all user
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = userService.GetAll();
            var usersDtos = mapper.Map<List<UserDto>>(users);
            return Ok(usersDtos);
        }

        //Get by id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = userService.GetById(id);
            var usersDtos = mapper.Map<UserDto>(user);
            return Ok(user);
        }

        //Get all user
        [HttpGet("FullUsers")]
        public IActionResult GetFullUsers()
        {
            var users = userService.GetAll();
            return Ok(users);
        }
        //Admin Tools=======


        //Get User Id
        [HttpGet("GetUserId")]
        public IActionResult GetUserId()
        {
            var CurrentUserId =int.Parse( HttpContext.User.Identity.Name );
            var user = userService.GetById(CurrentUserId);
            return Ok(CurrentUserId );
        }

        //Create a friend
        [HttpPost("addfriend")]
        public IActionResult AddFreindToMe([FromBody]FriendRequestDto friendRequestDto)
        {
            var CurrentUserId = int.Parse(HttpContext.User.Identity.Name);
            friendRequestDto.userId = CurrentUserId;
            try
            {
                userService.AddFreindToMe(friendRequestDto);
                return Ok("added");
            }
            catch (AppException ex)
            {
                return BadRequest(ex);
            }
        }

        //generateinvitation
        [HttpGet("generateinvitation")]
        public IActionResult GenerateInv()
        {
            var CurrentUserId = int.Parse(HttpContext.User.Identity.Name);
            try
            {
                var invCode = userService.GenerateUserInvitaionCode(CurrentUserId);
                return Ok(invCode);

            }
            catch (AppException ex)
            {

                return BadRequest(ex);
            }
            
        }

        //Edit User
        [HttpPut("editUser")]
        public IActionResult UpdateUser(UserDto userDto)
        {
            var user = mapper.Map<User>(userDto);
            var CurrentUserId = int.Parse(HttpContext.User.Identity.Name);
            user.id = CurrentUserId;
            try
            {
                var editeduser = userService.Update(user, userDto.password);
                return Ok(editeduser);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message, Exeption = ex });
            }

        }
        //Delete User
        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser()
        {
            var CurrentUserId = int.Parse(HttpContext.User.Identity.Name);
            try
            {
                var editeduser = userService.Delete(CurrentUserId);
                return Ok(editeduser);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message, Exeption = ex });
            }
        }


        // Anonumous
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult AuthenticatePost([FromBody]UserDto userDto)
        {
            var user = userService.Authenticate(userDto.username, userDto.password);
            //If user not found
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            //If user found
            var GenerateDto = mapper.Map<UserDto>(user);
            GenerateDto.token = userService.GenerateUserToken(user.id,7);
            return Ok(GenerateDto);
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult RegisterUser([FromBody]UserDto userDto)
        {
            var user = mapper.Map<User>(userDto);
            try
            {
                var AddedUser = userService.Create(user, userDto.password);
                return Ok(AddedUser);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message, Exeption = ex });
            }
        }
        //Dos User Exsits
        [AllowAnonymous]
        [HttpPost("UserExsits")]
        public IActionResult DoesUserExsits([FromBody]UserDto userDto)
        {
            var state = userService.IsUserNameExistsInDb(userDto.username);
            return Ok(state);
        }
    }//class
}
