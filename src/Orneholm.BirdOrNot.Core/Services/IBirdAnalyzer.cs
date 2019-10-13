using System.Threading.Tasks;
using Orneholm.BirdOrNot.Core.Models;

namespace Orneholm.BirdOrNot.Core.Services
{
    public interface IBirdAnalyzer
    {
        Task<BirdAnalysisResult> AnalyzeImageFromUrlAsync(string url);
    }
}