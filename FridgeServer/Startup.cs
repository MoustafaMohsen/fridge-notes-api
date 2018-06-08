using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FridgeServer.Data;
using Microsoft.AspNetCore;
using FridgeServer.Services;
using Microsoft.Extensions.Hosting;

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
            
            /*
           services.AddCors(options => options.AddPolicy("Cors"
                        , builder => {
                            builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader().AllowCredentials();
                        }));
           */ 
           // services.AddCors();
            var connection = @"Server=(localdb)\mssqllocaldb;Database=FridgeServer.AspNetCore.NewDb;Trusted_Connection=True;ConnectRetryCount=0";
            services.AddDbContext<AppDbContext>(
                options => {
                    //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                    options.UseSqlServer(connection);
                    //var config = Configuration["Data:DefaultConnection:ConnectionString"];

                    //options.UseInMemoryDatabase("Grocery")
                }
            );

            services.AddSingleton<IHostedService,FinalTest>(s => new FinalTest(new AppDbContext(null, connection) ));
            services.AddTransient<GuessTimeout>();
            //enable cors for angular


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());

            //app.UseCors("Cors");

            /*
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                );
                */
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

            }
            

            app.UseMvc();
        }
    }
}
