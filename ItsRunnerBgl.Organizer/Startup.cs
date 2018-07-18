using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItsRunnerBgl.Models.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ItsRunnerBgl.Organizer.Data;
using ItsRunnerBgl.Organizer.Models;
using ItsRunnerBgl.Organizer.Services;
using ItsRunnerBgl.Utility;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace ItsRunnerBgl.Organizer
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<IEventHubManager>(new EventHubManager(Configuration["EventHubConnectionString"], Configuration["EventHubToOrganizerEntityPath"]));
            // Data-specific things are stored in their own DB
            var sqlConnectionString = Configuration.GetConnectionString("DataConnection");
            services.AddSingleton<IUserRepository>(new UserRepository(sqlConnectionString));
            services.AddSingleton<ITelemetryRepository>(new TelemetryRepository(sqlConnectionString));
            services.AddSingleton<IActivityRepository>(new ActivityRepository(sqlConnectionString));

            services.AddSingleton<ISignalRRegistry, SignalRRegistry>();
            services.AddSingleton<IHostedService, OrganizerService>();
            services.AddMvc();
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseSignalR(routes =>
            {
                routes.MapHub<OrganizerHub>("/toorganizer");
            });
        }
    }
}
