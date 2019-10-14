namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisAnimal
    {
        public string Species { get; set; }
        public double? SpeciesConfidence { get; set; }
        public bool IsBird { get; set; }
        public double? IsBirdConfidence { get; set; }
        public bool IsAnimal { get; set; }
        public double? IsAnimalConfidence { get; set; }

        public BoundingRect Rectangle { get; set; }
    }
}