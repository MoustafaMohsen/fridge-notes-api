/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FridgeServer.Data;
using FridgeServer.Services;
using Microsoft.EntityFrameworkCore;
/*
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
*/

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
            //=========Sql Server
           
            services.AddDbContext<AppDbContext>(
                options => {
                    //Check AppContext if Edited
                    options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
                }
            );
            /*
            //=========MySql

            services.AddDbContext<AppDbContext>(
                options => {
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                    //options.UseSqlServer(connection);
                }
            );
            */
            //==========Sql Lite
            /*
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "MyDb.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            services.AddDbContext<AppDbContext>(
                options => {
                    options.UseSqlite(connection);
                }
            );
            */
            services.AddTransient<GuessTimeout>();
            services.AddMvc();
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
            app.UseMvc();
        }
    }
}
