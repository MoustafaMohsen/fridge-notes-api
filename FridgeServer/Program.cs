using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FridgeServer.Services;
namespace FridgeServer
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
           
            BuildWebHost(args).Run();
           // MyService.test();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.UseSetting("https_port", "6291")
                // .UseDefaultServiceProvider(options =>options.ValidateScopes = false)
                .Build();
    }
}
