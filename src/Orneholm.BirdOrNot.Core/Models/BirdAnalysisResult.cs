using System.Collections.Generic;
using System.Linq;

namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisResult
    {
        public static readonly BirdAnalysisResult Empty = new BirdAnalysisResult();

        public bool IsBird { get; set; }
        public double? IsBirdConfidence { get; set; }
        public string IsBirdText { get; set; } = string.Empty;

        public bool HasAnimalGroup => AnimalGroups.Any();
        public List<string> AnimalGroups { get; set; } = new List<string>();
        public List<BirdAnalysisAnimal> Animals { get; set; } = new List<BirdAnalysisAnimal>();

        public bool IsInappropriateContent { get; set; }
        public BirdAnalysisMetadata Metadata { get; set; } = new BirdAnalysisMetadata();
    }
}
