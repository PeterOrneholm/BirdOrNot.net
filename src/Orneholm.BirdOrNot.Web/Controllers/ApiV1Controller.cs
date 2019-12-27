using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Orneholm.BirdOrNot.Core.Models;
using Orneholm.BirdOrNot.Core.Services;

namespace Orneholm.BirdOrNot.Web.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class ApiV1Controller : Controller
    {
        private readonly IBirdAnalyzer _birdAnalyzer;
        private readonly TelemetryClient _telemetryClient;

        public ApiV1Controller(IBirdAnalyzer birdAnalyzer, TelemetryClient telemetryClient)
        {
            _birdAnalyzer = birdAnalyzer;
            _telemetryClient = telemetryClient;
        }

        [HttpGet("analyze")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<BirdAnalysisResult>> Index(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest();
            }

            try
            {
                var result = await _birdAnalyzer.AnalyzeImageFromUrlAsync(imageUrl);
                _telemetryClient.TrackEvent("BON_ImageAnalyzed", new Dictionary<string, string>
                    {
                        { "BON_ImageUrl", imageUrl },
                        { "BON_IsBird", result.IsBird.ToString() },
                        { "BON_BirdCount", result.Animals.Count.ToString() },
                        { "BON_IsBirdConfidence", result.IsBirdConfidence?.ToString() ?? string.Empty },
                        { "BON_ImageDescription", result.Metadata.ImageDescription },
                        { "BON_IsInappropriateContent", result.IsInappropriateContent.ToString() },
                        { "BON_Source", "Api_V1" }
                    });

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex, new Dictionary<string, string>
                    {
                        { "BON_ImageUrl", imageUrl },
                        { "BON_Source", "Api_V1" }
                    });

                return StatusCode((int)HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
