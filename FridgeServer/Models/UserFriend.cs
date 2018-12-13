using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models
{
    public class UserFriend
    {
        public int id { get; set; }
        public string friendUsername { get; set; }
        public string friendUserId { get; set; }
        public string friendEncryptedCode { get; set; }
        public bool AreFriends { get; set; }

        //Foreign Key
        public string ApplicationUserId { get; set; }
    }
}
