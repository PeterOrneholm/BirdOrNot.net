using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Orneholm.BirdOrNot.Web.Services
{
    public interface IBirdComputerVision
    {
        Task<ImageAnalysis> AnalyzeImageFromUrlAsync(string url);
    }
}