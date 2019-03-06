using MLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class InvitationDto
    {
        public double expire { get; set; }
        public string userId { get; set; }
    }

    public class InvitationResult
    {
        public string userId { get; set; }
        public string errors { get; set; } = null;
        public bool isSuccessful => errors == null ? true : false;

    }
}
