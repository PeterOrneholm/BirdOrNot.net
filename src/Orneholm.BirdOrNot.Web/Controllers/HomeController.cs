using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Orneholm.BirdOrNot.Web.Models;
using Orneholm.BirdOrNot.Web.Services;

namespace Orneholm.BirdOrNot.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly Dictionary<string, string> _samples = new Dictionary<string, string>
        {
            { "Hummingbird", "https://upload.wikimedia.org/wikipedia/commons/9/9b/Sparkling_Violet-ear.jpg" },
            { "Penguin", "https://upload.wikimedia.org/wikipedia/commons/0/07/Emperor_Penguin_Manchot_empereur.jpg" },
            { "Falcon", "https://upload.wikimedia.org/wikipedia/commons/9/9a/Falco_peregrinus_good_-_Christopher_Watson.jpg" },
            { "Crow", "https://upload.wikimedia.org/wikipedia/commons/b/b8/Nebelkr%C3%A4he_Corvus_cornix.jpg" },
            { "Alligator", "https://upload.wikimedia.org/wikipedia/commons/c/ca/Alligatoren_%28Alligator_mississippiensis%29_in_Florida.jpg" },
            { "Multiple Owls", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/80/Athene_cunicularia_20110524_02.jpg/2560px-Athene_cunicularia_20110524_02.jpg" }
        };

        private readonly IBirdAnalyzer _birdAnalyzer;
        private readonly TelemetryClient _telemetryClient;

        public HomeController(IBirdAnalyzer birdAnalyzer, TelemetryClient telemetryClient)
        {
            _birdAnalyzer = birdAnalyzer;
            _telemetryClient = telemetryClient;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<BirdAnalysisResult>> Index(string imageUrl)
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
                        { "BON_IsBirdConfidence", result.IsBirdConfidence.ToString() },
                        { "BON_ImageDescription", result.Metadata.ImageDescription },
                    });
                }
                catch (Exception ex)
                {
                    viewModel.Result = null;
                    _telemetryClient.TrackException(ex, new Dictionary<string, string>
                    {
                        { "BON_ImageUrl", imageUrl }
                    });
                }
            }

            viewModel.IsBirdText = GetIsBirdText(viewModel);
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

        private string GetIsBirdText(HomeIndexViewModel model)
        {
            if (!model.HasResult)
            {
                return string.Empty;
            }

            var result = model.Result;
            if (result.IsBird)
            {
                var birdSpiecies = result.Animals.FirstOrDefault(x => x.Spiecies != null);
                if (birdSpiecies != null)
                {
                    return $"It's a bird ({birdSpiecies.Spiecies})!";
                }

                return "It's a bird!";
            }

            return "It's not a bird.";
        }
    }
}
