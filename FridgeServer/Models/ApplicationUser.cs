using CoreUserIdentity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models
{
    public class ApplicationUser: MyIdentityUser
    {
        public string secretId { get; set; }
        public List<Grocery> userGroceries { get; set; }
        public List<UserFriend> userFriends { get; set; }

    }
}
