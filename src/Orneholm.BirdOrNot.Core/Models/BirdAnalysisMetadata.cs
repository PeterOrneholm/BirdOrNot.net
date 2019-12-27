using System.Collections.Generic;

namespace Orneholm.BirdOrNot.Core.Models
{
    public class BirdAnalysisMetadata
    {
        public List<string> ImageTags { get; set; } = new List<string>();
        public string ImageDescription { get; set; } = string.Empty;

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public string ImageFormat { get; set; } = string.Empty;
    }
}
