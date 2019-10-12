using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orneholm.BirdOrNot.Web.Models;
using Orneholm.BirdOrNot.Web.Services;

namespace Orneholm.BirdOrNot.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHealthChecks();
            services.Configure<BirdAnalysisOptions>(Configuration);
            services.Configure<GoogleAnalyticsOptions>(Configuration);
            services.AddTransient<IBirdAnalyzer, BirdAnalyzer>();
            services.AddApplicationInsightsTelemetry(options => { options.DeveloperMode = false; });
            services.AddApplicationInsightsTelemetryProcessor<ExcludeHealthDependencyFilter>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
