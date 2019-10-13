using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Orneholm.BirdOrNot.Core.Services
{
    public interface IBirdComputerVision
    {
        Task<ImageAnalysis> AnalyzeImageFromUrlAsync(string url);
    }
}