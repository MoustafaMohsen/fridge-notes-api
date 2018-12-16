using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using FridgeServer.Data;
using FridgeServer.EmailService;
using FridgeServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using FridgeServer.Models;
using FridgeServer.Models.Admin;

namespace FridgeServer._UserIdentity
{
    public interface IRunOnAppStart
    {
        Task<SiteStatus> Start();
        Task CreateDefaultRoles();
        Task<SiteStatus> CreateAdmin();

    }
    public class RunOnAppStart : IRunOnAppStart
    {
        #region Protected Members
        protected AppSettings appSettings;
        protected UserManager<ApplicationUser> mUserManager;
        protected SignInManager<ApplicationUser> mSignInManager;
        protected RoleManager<IdentityRole> roleManager;
        #endregion
        #region Constructor
        public RunOnAppStart(
            IOptions<AppSettings> _options,
            UserManager<ApplicationUser> _mUserManager,
            SignInManager<ApplicationUser> _mSignInManager,
            RoleManager<IdentityRole> _roleManager
            )
        {
            mUserManager = _mUserManager;
            mSignInManager = _mSignInManager;
            appSettings = _options.Value;
            roleManager = _roleManager;
        }
        #endregion

        public async Task<SiteStatus> Start()
        {
            //Create Roles
            await CreateDefaultRoles();
            //Create admin
            return await CreateAdmin();
        }

        #region Roles Creating
        private async Task _CreateRole(string name)
        {
            var role = new IdentityRole();
            role.Name = name;
            try
            {
                var results = await roleManager.CreateAsync(role);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw ex;
            }
        }

        private async Task<bool> RoleExsists(string roleName)
        {
            bool exsists;
            try
            {
                exsists = await roleManager.RoleExistsAsync(roleName);

            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                return false;
            }
            return exsists;
        }

        public async Task CreateDefaultRoles()
        {
            if (!await RoleExsists(MyRoles.admin))
                await _CreateRole(MyRoles.admin);

            if (!await RoleExsists(MyRoles.client))
                await _CreateRole(MyRoles.client);

            if (!await RoleExsists(MyRoles.manager))
                await _CreateRole(MyRoles.manager);

            if (!await RoleExsists(MyRoles.restricted))
                await _CreateRole(MyRoles.restricted);

            if (!await RoleExsists(MyRoles.unverfied))
                await _CreateRole(MyRoles.unverfied);
        }
        #endregion

        #region Create Admin
        public async Task<SiteStatus> CreateAdmin()
        {
            var adminUesr = await mUserManager.FindByEmailAsync(appSettings.adminInfo.email);
            if (adminUesr==null)
            {
                var user = new ApplicationUser()
                {
                    FirstName = appSettings.adminInfo.firstName,
                    LastName = appSettings.adminInfo.lastName,
                    UserName = appSettings.adminInfo.username,
                    Email = appSettings.adminInfo.email,
                    EmailConfirmed = true
                };
                var password = appSettings.adminInfo.password;

                var identityResult =await mUserManager.CreateAsync(user, password);

                if (identityResult.Succeeded)
                {
                    var AdminUser = await mUserManager.FindByEmailAsync(appSettings.adminInfo.email);
                    // adding admin role to admin user
                    try
                    {
                        await mUserManager.AddToRoleAsync(AdminUser, MyRoles.admin);
                        var FinalAdminUser = await mUserManager.FindByEmailAsync(appSettings.adminInfo.email);

                        var sitestatus = new SiteStatus();
                        sitestatus.Admin = FinalAdminUser.Email;
                        sitestatus.Alreadyrun = false;
                        sitestatus.DatabaseStatus = "ok";
                        return sitestatus;
                    }
                    catch (Exception ex)
                    {
                        if (Debugger.IsAttached)
                            Debugger.Break();
                        throw ex;
                    }
                }
                else
                {
                    throw new AppException("Couldn't create admin");
                }
            }//if
            else
            {
                await AddAdminRole(adminUesr);
                var FinalAdminUser = await mUserManager.FindByEmailAsync(appSettings.adminInfo.email);

                var sitestatus = new SiteStatus();
                sitestatus.Admin = FinalAdminUser.Email;
                sitestatus.Alreadyrun = true;
                sitestatus.DatabaseStatus = "ok";

                return sitestatus;
            }
        }
        #endregion

        #region Roles Creating
        private async Task<bool> hasAdminRole(ApplicationUser adminUesr)
        {
            var _role = await mUserManager.GetRolesAsync(adminUesr);
            var role = _role.ToList();
            return  role.Contains(MyRoles.admin);
        }

        private async Task AddAdminRole(ApplicationUser adminUesr)
        {
            if (await hasAdminRole(adminUesr) == false)
            {
                //Add admin role
                try
                {
                    await mUserManager.AddToRoleAsync(adminUesr, MyRoles.admin);
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw ex;
                }
            }

        }




        #endregion
    }//Class
}
