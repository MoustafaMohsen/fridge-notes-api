using MLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class FridgeExternalRegisterLoginResults
    {
        public UserDto User { get; set; }
        public string operation { get; set; } = null;
        public string errors { get; set; } = null;
        public string errorsDescription { get; set; } = null;
        public bool isSuccessful => M.isNull(errors) ? true : false;

    }
}
