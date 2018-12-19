using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VerficationEmailSender.Models;
using static CoreUserIdentity.Models.CoreUserAppSettings;

namespace FridgeServer.Helpers
{
    /*
    public class JwtAppSettings
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
    public class SendGridAppSettings
    {
        public string key { get; set; }
        public string user { get; set; }
    }
    public class EmailVerficationInfo
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
    }
    public class AdminInfo
    {
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
    */
    public class AppSettings
    {
        public SendGridAppSettings sendGrid { get; set; }
        public JwtAppSettings jwt { get; set; }
        public EmailSettings emailSettings { get; set; }
        public AdminInfo adminInfo { get; set; }
        public string apphost { get; set; }
        public string appVerPath { get; set; }
    }
}
