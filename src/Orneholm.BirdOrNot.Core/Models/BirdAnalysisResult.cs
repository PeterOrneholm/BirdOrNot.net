using System.Collections.Generic;
using System.Linq;

namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisResult
    {
        public bool IsBird { get; set; }
        public bool IsInappropriateContent { get; set; }
        public double? IsBirdConfidence { get; set; }

        public bool HasSpecies => Species.Any();
        public List<string> Species { get; set; }

        public List<BirdAnalysisAnimal> Animals { get; set; }

        public BirdAnalysisMetadata Metadata { get; set; }
    }
}