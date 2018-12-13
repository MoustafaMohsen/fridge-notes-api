using FridgeServer._UserIdentity;
using FridgeServer.Models._UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class UserDto : _IdentityUserDto
    {
    
        public List<UserFriend> userFriends { get; set; }
        public string invitationcode { get; set; }
    }
}
