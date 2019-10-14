using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Orneholm.BirdOrNot.Core.Services
{
    public class BirdComputerVision : IBirdComputerVision
    {
        private readonly IComputerVisionClient _computerVisionClient;
        private static readonly List<VisualFeatureTypes> VisualFeatures = new List<VisualFeatureTypes>
        {
            // 8.738 SEK / 1000 transactions
            VisualFeatureTypes.Tags,

            // 13.106 SEK / 1000 transactions
            VisualFeatureTypes.Adult,
            VisualFeatureTypes.Objects,

            // 21.843 SEK / 1000 transactions
            VisualFeatureTypes.Description
        };

        //Total: 0,056793 SEK / image

        public BirdComputerVision(IComputerVisionClient computerVisionClient)
        {
            _computerVisionClient = computerVisionClient;
        }

        public async Task<ImageAnalysis> AnalyzeImageFromUrlAsync(string url)
        {
            return await _computerVisionClient.AnalyzeImageAsync(url, VisualFeatures);
        }
    }
}
