using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.EmailService.SendGrid
{
    public static class SendGrindExtention
    {
        public static IServiceCollection AddMySendGrindEmail(this IServiceCollection services)
        {
            services.AddScoped<ISendgrindEmailSender, SendgrindEmailSender>();
            return services;
        }
    }
}
