using CoreUserIdentity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models
{
    public class ApplicationUser: MyIdentityUser
    {
        [MaxLength(length: 512)]
        public string secretId { get; set; }

        public List<Grocery> userGroceries { get; set; }

        public List<UserFriend> userFriends { get; set; }

    }
}
