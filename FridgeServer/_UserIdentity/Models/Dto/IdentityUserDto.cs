using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models._UserIdentity
{
    
    public class _IdentityUserDto
    {
        #region Public Properties
        /// <summary>
        /// The users Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The users first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The users last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The users username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The users email
        /// </summary>
        public string Email { get; set; }

        public string password { get; set; } = "";

        /// <summary>
        /// The users role
        /// </summary>
        public string role { get; set; }

        /// <summary>
        /// The authentication token used to stay authenticated through future requests
        /// </summary>
        /// <remarks>The Token is only provided when called from the login methods</remarks>
        public string token { get; set; }
        #endregion
    }
    
}
