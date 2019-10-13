using System.Collections.Generic;

namespace Orneholm.BirdOrNot.Web.Models
{
    public class BirdAnalysisResult
    {
        public bool IsBird { get; set; }
        public double? IsBirdConfidence { get; set; }

        public List<BirdAnalysisAnimal> Animals { get; set; }

        public BirdAnalysisMetadata Metadata { get; set; }
    }
}