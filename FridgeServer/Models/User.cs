using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models
{
    public class User
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public byte[] passwordHash { get; set; }
        public byte[] passwordSalt { get; set; }

        public string SecretId { get; set; }
        public List<Grocery> UserGroceries { get; set; }
        public List<UserFriend> friends { get; set; }
    }
    public class UserFriend
    {
        public int id { get; set; }
        public string friendUsername { get; set; }
        public int friendUserId { get; set; }
        public string friendEncryptedCode { get; set; }
        public bool AreFriends { get; set; }
    }
}
