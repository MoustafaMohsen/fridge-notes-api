using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FridgeServer.Data;
using FridgeServer.Services;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Helpers;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using AutoMapper;
using FridgeServer.EmailService;
using FridgeServer._UserIdentity;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.IO;

namespace FridgeServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            // path to sensetive appsettings jsons, keep out .git folder
            string PrivateAppsettiingsPath = "";
            var pathtest = Path.Combine(env.ContentRootPath, "AppSettings");


            // if in Development
            if (env.IsDevelopment())
            {
                PrivateAppsettiingsPath = Path.Combine(env.ContentRootPath, "..", "..", "Data", "AppSettings");
            }
            // if in Production
            if (env.IsProduction())
            {
                PrivateAppsettiingsPath = Path.Combine(env.ContentRootPath, "AppSettings");
            }
            // if in Staging
            if (env.IsStaging())
            {
                PrivateAppsettiingsPath = Path.Combine(env.ContentRootPath, "AppSettings");
            }
            // add settings files
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"{PrivateAppsettiingsPath}/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"{PrivateAppsettiingsPath}/adminsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"{PrivateAppsettiingsPath}/mailsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();

            //=========Sql Server
            services.AddDbContext<AppDbContext>(
                options => {
                    //Check AppContext if Edited
                    options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
                    //options.UseInMemoryDatabase("testDb");
                    //options.UseSqlServer(Configuration.GetConnectionString("LocalSqlServer"));
                }
            );

            //configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();

            //Add configration
            services.Configure<AppSettings>(appSettingsSection);


            // Add My Identity
            services.AddMyUserIdentity<AppDbContext>(appSettings.jwt.SecretKey, appSettings.jwt.Audience, appSettings.jwt.Issuer);


            services.AddScoped< IGroceriesService, GroceriesService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<GuessTimeout>();

            services.AddMvc()
                // Make api Return properties as it is, Keeps capital properties
               .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });

            services.AddCors();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            //show debug exception page in production
            if (env.IsProduction())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
