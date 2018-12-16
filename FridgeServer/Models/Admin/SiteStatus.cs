using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Admin
{
    public class SiteStatus
    {
        public string DatabaseStatus { get; set; }
        public string Admin { get; set; }
        public bool Alreadyrun { get; set; }
    }
}
