using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreUserIdentity.Models;

namespace FridgeServer.Models.Dto
{
    public class UserDto : _IdentityUserDto
    {
    
        public List<UserFriend> userFriends { get; set; }
        public string invitationcode { get; set; }
    }
}
