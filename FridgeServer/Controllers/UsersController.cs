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


        //Edit User
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, UserDto userDto)
        {
            var user = mapper.Map<User>(userDto);
            user.id = id;
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
        //DeleteUser
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var editeduser = userService.Delete(id);
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
