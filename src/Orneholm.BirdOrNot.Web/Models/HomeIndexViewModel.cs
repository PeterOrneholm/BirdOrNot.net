using System.Collections.Generic;

namespace Orneholm.BirdOrNot.Web.Models
{
    public class HomeIndexViewModel
    {
        public string ImageUrl { get; set; }
        public bool HasResult => Result != null;
        public bool IsInvalid => ImageUrl != null && Result == null;
        public BirdAnalysisResult Result { get; set; }

        public Dictionary<string, string> Samples { get; set; }

        public string IsBirdText { get; set; }
        public string CanonicalUrl { get; set; }
    }
}