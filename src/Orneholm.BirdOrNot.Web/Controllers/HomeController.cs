using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Orneholm.BirdOrNot.Core.Models;
using Orneholm.BirdOrNot.Core.Services;
using Orneholm.BirdOrNot.Web.Models;

namespace Orneholm.BirdOrNot.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly Dictionary<string, string> _samples = new Dictionary<string, string>
        {
            { "Hummingbird", "https://pobirdornotprod.blob.core.windows.net/samples/Sparkling_Violet-ear.jpg" },
            { "Penguin", "https://pobirdornotprod.blob.core.windows.net/samples/Emperor_Penguin_Manchot_empereur.jpg" },
            { "Falcon", "https://pobirdornotprod.blob.core.windows.net/samples/Falco_peregrinus_good_-_Christopher_Watson.jpg" },
            { "Crow", "https://pobirdornotprod.blob.core.windows.net/samples/Nebelkr%C3%A4he_Corvus_cornix.jpg" },
            { "Alligator", "https://pobirdornotprod.blob.core.windows.net/samples/Alligatoren_(Alligator_mississippiensis)_in_Florida.jpg" },
            { "Multiple Owls", "https://pobirdornotprod.blob.core.windows.net/samples/Athene_cunicularia_20110524_02.jpg" }
        };

        private readonly IBirdAnalyzer _birdAnalyzer;
        private readonly TelemetryClient _telemetryClient;

        public HomeController(IBirdAnalyzer birdAnalyzer, TelemetryClient telemetryClient)
        {
            _birdAnalyzer = birdAnalyzer;
            _telemetryClient = telemetryClient;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<HomeIndexViewModel>> Index(string imageUrl)
        {
            var viewModel = new HomeIndexViewModel
            {
                ImageUrl = imageUrl,
                Samples = _samples
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    var result = await _birdAnalyzer.AnalyzeImageFromUrlAsync(imageUrl);
                    viewModel.Result = result;
                    _telemetryClient.TrackEvent("BON_ImageAnalyzed", new Dictionary<string, string>
                    {
                        { "BON_ImageUrl", imageUrl },
                        { "BON_IsBird", result.IsBird.ToString() },
                        { "BON_BirdCount", result.Animals.Count.ToString() },
                        { "BON_IsBirdConfidence", result.IsBirdConfidence?.ToString() ?? string.Empty },
                        { "BON_ImageDescription", result.Metadata.ImageDescription },
                        { "BON_IsSample", _samples.Values.Contains(imageUrl).ToString() },
                        { "BON_IsInappropriateContent", result.IsInappropriateContent.ToString() },
                        { "BON_Source", "Site" }
                    });
                }
                catch (Exception ex)
                {
                    viewModel.Result = BirdAnalysisResult.Empty;
                    _telemetryClient.TrackException(ex, new Dictionary<string, string>
                    {
                        { "BON_ImageUrl", imageUrl },
                        { "BON_Source", "Site" }
                    });
                }
            }

            viewModel.IsBirdText = viewModel.Result?.IsBirdText ?? string.Empty;
            viewModel.CanonicalUrl = GetCanonicalUrl(viewModel);

            return View(viewModel);
        }

        private string GetCanonicalUrl(HomeIndexViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                return "https://birdornot.net/";
            }

            return $"https://birdornot.net/?ImageUrl={Uri.EscapeDataString(model.ImageUrl)}";
        }
    }
}
