using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
            services.AddApplicationInsightsTelemetry(options => { options.DeveloperMode = false; });
            services.AddApplicationInsightsTelemetryProcessor<ExcludeHealthDependencyFilter>();

            services.Configure<GoogleAnalyticsOptions>(Configuration);

            services.Configure<BirdAnalysisOptions>(Configuration);
            services.AddTransient<IBirdAnalyzer, BirdAnalyzer>();
            services.AddTransient<IBirdComputerVision, BirdComputerVision>();
            services.AddTransient<IComputerVisionClient>(services =>
            {
                var birdAnalysisOptions = services.GetService<IOptions<BirdAnalysisOptions>>();
                return new ComputerVisionClient(new ApiKeyServiceClientCredentials(birdAnalysisOptions.Value.AzureComputerVisionSubscriptionKey))
                {
                    Endpoint = birdAnalysisOptions.Value.AzureComputerVisionEndpoint
                };
            });
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
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
