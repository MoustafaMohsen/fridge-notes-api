using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class UserDto
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string token { get; set; }
    }
    public class FriendRequestDto
    {
        public string invetationCode { get; set; }
        public int userId { get; set; }

    }
}
