using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreUserIdentity._UserIdentity;
using CoreUserIdentity.Models;
using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using FridgeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace FridgeServer.Controllers
{
    static class setting
    {
        public static bool alreadyrun = false;
        public static SiteStatus siteStatus { get; set; }
    }

    [AuthTokenManager]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private IUserService userService;
        private IRunOnAppStart<ApplicationUser> runOnAppStart;
        private AppSettings appSettings;
        private readonly IHostingEnvironment env;
        private AppDbContext db;
        public AdminController(IUserService _userService, IRunOnAppStart<ApplicationUser> _runOnAppStart,IOptions<AppSettings> _options
            , IHostingEnvironment _hostingEnvironment, AppDbContext _db)
        {
            appSettings = _options.Value;
            userService = _userService;
            env = _hostingEnvironment;
            runOnAppStart = _runOnAppStart;
            db = _db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new { result = new string[] { "welcome admin", $"Id:{GetTokenId()}",$"name:{GetTokenUsername()}" } });
        }

        //Get all user
        [HttpGet("users")]
        public async Task<IActionResult> GetAll()
        {
            var users =userService.GetAllUsing_Manager().ToList();
            return Ok(new { users });
        }

        //Get by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string Id)
        {
            var user = await userService.GetById_Manager(Id);
            return Ok(user);
        }

        //Get all user
        [HttpGet("FullUsers")]
        public async Task<IActionResult> GetFullUsers()
        {
            var users = await userService.GetAllUsing_Db();
            return Ok(new { users });
        }


        //Run for the first time
        [AllowAnonymous]
        [HttpGet("runonfirsttime/{code}")]
        public async Task<IActionResult> RunForFirstTime(string code,[FromQuery(Name ="force")]bool force=false)
        {
            // check that the provided code is correct
            var admincode = appSettings.adminaccesscode;
            if (admincode != code)
                return NotFound();
            
            if (setting.alreadyrun==false || force)
            {
                setting.alreadyrun = true;
                if (env.IsProduction() )
                {
                    db.Database.Migrate();
                }
                var siteStatus = await runOnAppStart.Start();
                var appSettingsFile = appSettings;
                setting.siteStatus = siteStatus;
                var results = new { appSettingsFile, siteStatus };
                return Ok( ret(results, "admin created") );
            }
            else
            {
                var siteStatus = setting.siteStatus;
                var appSettingsFile = appSettings;
                var results = new { appSettingsFile, siteStatus };
                return Ok(ret(results, "admin already run"));
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
        public string GetTokenUsername()
        {
            var name = HttpContext.User.Identity.Name;
            if (name != null)
            {
                return name;
            }
            return null;
        }
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
