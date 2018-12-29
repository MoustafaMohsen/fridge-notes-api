using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models
{
    public class UserFriend
    {
        [Key]
        public string id { get; set; }

        [MaxLength(length: 255)]
        public string friendUsername { get; set; }

        [MaxLength(length: 255)]
        public string friendUserId { get; set; }

        [MaxLength(length: 255)]
        public string friendEncryptedCode { get; set; }

        public bool AreFriends { get; set; }

        //Foreign Key
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
    }
}
