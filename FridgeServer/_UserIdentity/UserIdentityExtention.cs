using FridgeServer.EmailService;
using FridgeServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeServer._UserIdentity
{
    public static class UserIdentityExtention 
    {
        public static IServiceCollection AddMyUserIdentity<TContext>(this IServiceCollection services, string jwtkey,string audience, string issuer) where TContext : DbContext
        {
            services.AddMyEmailVerfication();

            services.AddScoped<IUserIdentityManager, UserIdentityManager>();

            services.AddIdentity<ApplicationUser, IdentityRole>(
                a =>
                {
                    a.SignIn.RequireConfirmedEmail = true;
                    a.User.RequireUniqueEmail = true;
                    a.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+دجحخهعغفقثصضطكمنتالبيسشظزوةىرؤءئ";
                }
            ).AddEntityFrameworkStores<TContext>().AddDefaultTokenProviders();

            var key = Encoding.UTF8.GetBytes(jwtkey);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.AddScoped<IRunOnAppStart, RunOnAppStart>();
            return services;
        }
    }
}
