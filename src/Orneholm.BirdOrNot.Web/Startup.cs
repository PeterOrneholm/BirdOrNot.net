using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orneholm.BirdOrNot.Web.Models;
using Orneholm.BirdOrNot.Core.Services;

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

            services.AddTransient<IBirdAnalyzer, BirdAnalyzer>();
            services.AddTransient<BirdComputerVision>();
            services.AddTransient<IComputerVisionClient>(x =>
            {
                return new ComputerVisionClient(new ApiKeyServiceClientCredentials(Configuration["AzureComputerVisionSubscriptionKey"]))
                {
                    Endpoint = Configuration["AzureComputerVisionEndpoint"]
                };
            });
            services.AddTransient<IBirdComputerVision>(x => new CachedBirdComputerVision(x.GetService<BirdComputerVision>(), x.GetService<IDistributedCache>()));

            var redisConnectionString = Configuration["RedisConnectionString"];
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "BirdOrNot";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
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

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = GetContentTypeProvider()
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapDefaultControllerRoute();
            });
        }

        private static FileExtensionContentTypeProvider GetContentTypeProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            return provider;
        }
    }
}
