using System.Collections.Generic;

namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisMetadata
    {
        public List<string> ImageTags { get; set; }
        public string ImageDescription { get; set; }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public string ImageFormat { get; set; }
    }
}