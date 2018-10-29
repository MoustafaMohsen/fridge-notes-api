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


namespace FridgeServer
{
    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            //=========Sql Server
            services.AddDbContext<AppDbContext>(
                options => {
                    //Check AppContext if Edited
                    //options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
                    //options.UseInMemoryDatabase("testDb");
                    options.UseSqlServer(Configuration.GetConnectionString("LocalSqlServer"));
                }
            );

            //configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            //Add configration
            services.Configure<AppSettings>(appSettingsSection);

            //====configure jwt authentication
            //Get Key
            var appSetting = appSettingsSection.Get<AppSettings>();
            var Key = Encoding.ASCII.GetBytes(appSetting.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = int.Parse(context.Principal.Identity.Name);
                        var user = userService.FindById(userId);
                        if (user == null)
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }
                        return Task.CompletedTask;
                    }
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddScoped< IGroceriesService, GroceriesService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<GuessTimeout>();
            services.AddMvc();
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
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
