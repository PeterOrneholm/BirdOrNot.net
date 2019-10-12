using System.Threading.Tasks;
using Orneholm.BirdOrNot.Web.Models;

namespace Orneholm.BirdOrNot.Web.Services
{
    public interface IBirdAnalyzer
    {
        Task<BirdAnalysisResult> AnalyzeImageFromUrlAsync(string url);
    }
}