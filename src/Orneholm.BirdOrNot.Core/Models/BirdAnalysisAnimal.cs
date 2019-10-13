namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisAnimal
    {
        public string Spiecies { get; set; }
        public double? SpieciesConfidence { get; set; }
        public bool IsBird { get; set; }
        public double? IsBirdConfidence { get; set; }
        public bool IsAnimal { get; set; }
        public double? IsAnimalConfidence { get; set; }

        public BoundingRect Rectangle { get; set; }
    }
}