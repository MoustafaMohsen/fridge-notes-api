using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer._UserIdentity.Dto
{
    public class LoginUserDto
    {
        /// <summary>
        /// the login username or email
        /// </summary>
        public string usernameOrEmail { get; set; }

        /// <summary>
        /// the login password
        /// </summary>
        public string password { get; set; }
    }
}
