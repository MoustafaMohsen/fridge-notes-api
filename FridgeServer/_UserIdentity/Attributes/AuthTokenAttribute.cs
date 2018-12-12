using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using FridgeServer._UserIdentity;

namespace FridgeServer._UserIdentity
{
    public class AuthTokenAdminAttribute : AuthorizeAttribute
    {
        public AuthTokenAdminAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = $"{MyRoles.admin}";
        }
    }

    public class AuthTokenManagerAttribute : AuthorizeAttribute
    {
        public AuthTokenManagerAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = $"{MyRoles.admin},{MyRoles.manager}";
        }
    }

    public class AuthTokenClientAttribute : AuthorizeAttribute
    {
        public AuthTokenClientAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = $"{MyRoles.admin},{MyRoles.manager},{MyRoles.client}";
        }
    }

    public class AuthTokenUnverfiedAttribute : AuthorizeAttribute
    {
        public AuthTokenUnverfiedAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = $"{MyRoles.admin},{MyRoles.manager},{MyRoles.unverfied}";
        }
    }

    public class AuthTokenAnyAttribute : AuthorizeAttribute
    {
        public AuthTokenAnyAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }
    }

}
