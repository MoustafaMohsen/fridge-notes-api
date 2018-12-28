using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class UpdatePasswordDto
    {
        public string id { get; set; }
        public string oldpassword { get; set; }
        public string newpassword { get; set; }
        public string externalProvider { get; set; } = null;
    }
}
