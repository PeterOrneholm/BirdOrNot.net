using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Orneholm.BirdOrNot.Core.Models;

namespace Orneholm.BirdOrNot.Core.Services
{
    public class BirdAnalyzer : IBirdAnalyzer
    {
        private const string AnimalObjectKey = "animal";
        private const string BirdObjectKey = "bird";

        private readonly IBirdComputerVision _birdComputerVision;

        public BirdAnalyzer(IBirdComputerVision birdComputerVision)
        {
            _birdComputerVision = birdComputerVision;
        }

        public async Task<BirdAnalysisResult> AnalyzeImageFromUrlAsync(string url)
        {
            var analyzedImage = await _birdComputerVision.AnalyzeImageFromUrlAsync(url);

            if (IsInappropriateContent(analyzedImage))
            {
                return null;
            }

            return GetBirdAnalysisResult(analyzedImage);
        }

        private static bool IsInappropriateContent(ImageAnalysis analyzedImage)
        {
            return (analyzedImage.Adult.IsAdultContent && analyzedImage.Adult.AdultScore > 0.9)
                   || (analyzedImage.Adult.IsGoryContent && analyzedImage.Adult.GoreScore > 0.9)
                   || (analyzedImage.Adult.IsRacyContent && analyzedImage.Adult.RacyScore > 0.9);
        }

        private static BirdAnalysisResult GetBirdAnalysisResult(ImageAnalysis analyzedImage)
        {
            var birds = GetBirds(analyzedImage).ToList();
            var imageDescription = MakeSentence(analyzedImage.Description?.Captions?.FirstOrDefault()?.Text);

            return new BirdAnalysisResult
            {
                IsBird = birds.Any(x => x.IsBird),
                IsBirdConfidence = birds.Where(x => x.IsBird).Max(x => x.IsBirdConfidence),
                Animals = birds,
                Metadata = new BirdAnalysisMetadata
                {
                    ImageDescription = imageDescription,
                    ImageTags = analyzedImage.Description?.Tags.ToList()
                }
            };
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpper(value[0]) + value.Substring(1);
        }

        private static string MakeSentence(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Capitalize(value) + ".";
        }

        private static IEnumerable<BirdAnalysisAnimal> GetBirds(ImageAnalysis analyzedImage)
        {
            return analyzedImage.Objects.ToDictionary(x => x, GetObjectHierarchy)
                .Where(x => x.Value.ContainsKey(AnimalObjectKey))
                .Select(x =>
                {
                    var first = x.Value.FirstOrDefault();
                    var hasSpiecies = x.Value.Count >= 3;

                    var isAnimal = x.Value.ContainsKey(AnimalObjectKey);
                    var isBird = x.Value.ContainsKey(BirdObjectKey);

                    return new BirdAnalysisAnimal
                    {
                        Spiecies = hasSpiecies ? Capitalize(first.Key) : null,
                        SpieciesConfidence = hasSpiecies ? first.Value : (double?)null,

                        IsAnimal = isAnimal,
                        IsAnimalConfidence = isAnimal ? x.Value[AnimalObjectKey] : (double?)null,

                        IsBird = isBird,
                        IsBirdConfidence = isBird ? x.Value[BirdObjectKey] : (double?)null,

                        Rectangle = new Models.BoundingRect
                        {
                            x = x.Key.Rectangle.X,
                            y = x.Key.Rectangle.Y,
                            w = x.Key.Rectangle.W,
                            h = x.Key.Rectangle.H
                        }
                    };
                });
        }

        private static Dictionary<string, double> GetObjectHierarchy(DetectedObject detectedObject)
        {
            var objectHierarchy = new Dictionary<string, double>
            {
                { detectedObject.ObjectProperty, detectedObject.Confidence }
            };

            var currentObject = detectedObject.Parent;
            while (currentObject != null)
            {
                objectHierarchy.Add(currentObject.ObjectProperty, currentObject.Confidence);
                currentObject = currentObject.Parent;
            }

            return objectHierarchy;
        }
    }
}
