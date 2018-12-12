using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.EmailService.SendGrid
{
    public class SendGrindResponse
    {
        public List<SendGridResponseError> Errors { get; set; }
    }

    public class SendEmailResponse
    {
        public List<string> Errors { get; set; }
        public bool isSuccesful => Errors == null ? true : false;
    }
}
