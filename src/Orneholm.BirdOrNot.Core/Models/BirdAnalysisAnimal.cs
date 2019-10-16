namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisAnimal
    {
        public string AnimalGroup { get; set; }
        public double? AnimalGroupConfidence { get; set; }
        public bool IsBird { get; set; }
        public double? IsBirdConfidence { get; set; }
        public bool IsAnimal { get; set; }
        public double? IsAnimalConfidence { get; set; }

        public BoundingRect Rectangle { get; set; }
    }
}