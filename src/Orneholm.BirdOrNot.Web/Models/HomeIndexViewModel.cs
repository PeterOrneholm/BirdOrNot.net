using System.Collections.Generic;
using Orneholm.BirdOrNot.Core.Models;

namespace Orneholm.BirdOrNot.Web.Models
{
    public class HomeIndexViewModel
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool HasResult => Result != BirdAnalysisResult.Empty;
        public bool IsInvalid => ImageUrl != null && Result == BirdAnalysisResult.Empty;
        public BirdAnalysisResult Result { get; set; } = BirdAnalysisResult.Empty;

        public Dictionary<string, string> Samples { get; set; } = new Dictionary<string, string>();

        public string IsBirdText { get; set; } = string.Empty;
        public string CanonicalUrl { get; set; } = string.Empty;
    }
}
