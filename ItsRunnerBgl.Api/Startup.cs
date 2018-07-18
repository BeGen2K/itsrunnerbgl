using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ItsRunnerBgl.Api.Data;
using ItsRunnerBgl.Api.Models;
using ItsRunnerBgl.Api.Services;
using ItsRunnerBgl.Models.Repositories;
using ItsRunnerBgl.Utility;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace ItsRunnerBgl.Api
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
            var sqlConnectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(sqlConnectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<IQueueManager>(new QueueManager(Configuration["StorageConnectionString"]));
            //services.AddSingleton<IServiceBusManager>(new ServiceBusManager(Configuration["StorageConnectionString"]));
            services.AddSingleton<IEventHubManager>(new EventHubManager(Configuration["EventHubConnectionString"], Configuration["EventHubToWorkerEntityPath"]));
            services.AddSingleton<IUserRepository>(new UserRepository(sqlConnectionString));
            services.AddSingleton<ITelemetryRepository>(new TelemetryRepository(sqlConnectionString));
            services.AddSingleton<IActivityRepository>(new ActivityRepository(sqlConnectionString));


            services.AddMvc();
            services.AddSingleton<ISignalRRegistry, SignalRRegistry>();
            services.AddSingleton<IHostedService, CommandResponsesService>();
            services.AddSingleton<IHostedService, OrganizerService>();

            services.AddSignalR();
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
                routes.MapHub<CommandResponsesHub>("/torunner");
                routes.MapHub<OrganizerHub>("/toorganizer");
            });
        }
    }
}
