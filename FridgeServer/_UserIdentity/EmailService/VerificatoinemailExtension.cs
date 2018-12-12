using Microsoft.Extensions.DependencyInjection;
using FridgeServer.EmailService.SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.EmailService
{
    public static class VerificatoinemailExtension
    {
        public static IServiceCollection AddMyEmailVerfication(this IServiceCollection services)
        {
            services.AddMySendGrindEmail();
            services.AddScoped<IVerificationEmail, VerificationEmail>();
            return services;
        }
    }
}
