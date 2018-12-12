using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text;
using FridgeServer.Models;

namespace FridgeServer._UserIdentity
{
    public class MyIdentityUser : IdentityUser
    {
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
    }
}
