using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Orneholm.BirdOrNot.Web.Services
{
    public class CachedBirdComputerVision : IBirdComputerVision
    {
        private const string CacheKeyPrefix = "BON_AnalyzeUrl_V1";
        private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromDays(30);

        private readonly IBirdComputerVision _birdComputerVision;
        private readonly IDistributedCache _distributedCache;

        public CachedBirdComputerVision(IBirdComputerVision birdComputerVision, IDistributedCache distributedCache)
        {
            _birdComputerVision = birdComputerVision;
            _distributedCache = distributedCache;
        }

        public async Task<ImageAnalysis> AnalyzeImageFromUrlAsync(string url)
        {
            var imageAnalysis = await GetCache(url);
            if (imageAnalysis == null)
            {
                imageAnalysis = await _birdComputerVision.AnalyzeImageFromUrlAsync(url);
                await SetCache(url, imageAnalysis);
            }

            return imageAnalysis;
        }

        private async Task<ImageAnalysis> GetCache(string url)
        {
            var encodedImageAnalysis = await _distributedCache.GetAsync(GetKey(url));
            if (encodedImageAnalysis != null)
            {
                var jsonImageAnalysis = Encoding.UTF8.GetString(encodedImageAnalysis);
                return JsonConvert.DeserializeObject<ImageAnalysis>(jsonImageAnalysis);
            }

            return null;
        }

        private async Task SetCache(string url, ImageAnalysis imageAnalysis)
        {
            var jsonImageAnalysis = JsonConvert.SerializeObject(imageAnalysis);
            var encodedImageAnalysis = Encoding.UTF8.GetBytes(jsonImageAnalysis);
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = CacheSlidingExpiration
            };
            await _distributedCache.SetAsync(GetKey(url), encodedImageAnalysis, options);
        }

        private string GetKey(string url)
        {
            return $"{CacheKeyPrefix}_{url}";
        }
    }
}