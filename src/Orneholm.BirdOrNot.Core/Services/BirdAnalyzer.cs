using System;
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
            var birdAnalysis = GetBirdAnalysisResult(analyzedImage);

            birdAnalysis.IsInappropriateContent = IsInappropriateContent(analyzedImage);

            return birdAnalysis;
        }

        private static bool IsInappropriateContent(ImageAnalysis analyzedImage)
        {
            return (analyzedImage.Adult.IsAdultContent && analyzedImage.Adult.AdultScore > 0.95)
                   || (analyzedImage.Adult.IsGoryContent && analyzedImage.Adult.GoreScore > 0.95)
                   || (analyzedImage.Adult.IsRacyContent && analyzedImage.Adult.RacyScore > 0.95);
        }

        private static BirdAnalysisResult GetBirdAnalysisResult(ImageAnalysis analyzedImage)
        {
            var animals = GetAnimals(analyzedImage).ToList();
            var imageDescription = MakeSentence(analyzedImage.Description?.Captions?.FirstOrDefault()?.Text);

            var birdAnalysisMetadata = new BirdAnalysisMetadata
            {
                ImageDescription = imageDescription,
                ImageTags = analyzedImage.Description?.Tags.ToList(),

                ImageWidth = analyzedImage.Metadata.Width,
                ImageHeight = analyzedImage.Metadata.Height,

                ImageFormat = analyzedImage.Metadata.Format
            };

            UpdateRectangles(animals, birdAnalysisMetadata);

            var isBird = GetIsBird(analyzedImage, animals);
            return new BirdAnalysisResult
            {
                IsBird = isBird.Key,
                IsBirdConfidence = isBird.Value,

                Animals = animals,

                Metadata = birdAnalysisMetadata
            };
        }

        private static KeyValuePair<bool, double?> GetIsBird(ImageAnalysis analyzedImage, List<BirdAnalysisAnimal> animals)
        {
            var fromObjects = new KeyValuePair<bool, double?>(
                animals.Any(x => x.IsBird),
                animals.Where(x => x.IsBird).Max(x => x.IsBirdConfidence)
            );

            var birdTag = analyzedImage.Tags.FirstOrDefault(x => TextEqualsBird(x.Name));
            var fromTags = new KeyValuePair<bool, double?>(
                birdTag != null,
                birdTag?.Confidence
            );

            if (fromObjects.Key || fromTags.Key)
            {
                return new KeyValuePair<bool, double?>(
                    true,
                    Math.Max(fromObjects.Value.GetValueOrDefault(), fromTags.Value.GetValueOrDefault())
                );
            }

            var descriptionWords = analyzedImage.Description.Captions.SelectMany(x => x.Text.Split(' ')).ToList();
            var anyBirdWordInDescription = descriptionWords.Any(TextEqualsBird);

            return new KeyValuePair<bool, double?>(
                anyBirdWordInDescription,
                null
            );
        }

        private static bool TextEqualsBird(string x)
        {
            return x.Equals(BirdObjectKey, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void UpdateRectangles(List<BirdAnalysisAnimal> birds, BirdAnalysisMetadata birdAnalysisMetadata)
        {
            foreach (var bird in birds)
            {
                var rectangle = bird.Rectangle;

                rectangle.xPercentage = (double)rectangle.x / birdAnalysisMetadata.ImageWidth * 100;
                rectangle.yPercentage = (double)rectangle.y / birdAnalysisMetadata.ImageHeight * 100;

                rectangle.wPercentage = (double)rectangle.w / birdAnalysisMetadata.ImageWidth * 100;
                rectangle.hPercentage = (double)rectangle.h / birdAnalysisMetadata.ImageHeight * 100;
            }
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

        private static IEnumerable<BirdAnalysisAnimal> GetAnimals(ImageAnalysis analyzedImage)
        {
            return analyzedImage.Objects.ToDictionary(x => x, GetObjectHierarchy)
                .Where(x => x.Value.ContainsKey(AnimalObjectKey))
                .Select(x =>
                {
                    var first = x.Value.FirstOrDefault();
                    var hasSpecies = x.Value.Count >= 3;

                    var isAnimal = x.Value.ContainsKey(AnimalObjectKey);
                    var isBird = x.Value.ContainsKey(BirdObjectKey);

                    return new BirdAnalysisAnimal
                    {
                        Species = hasSpecies ? Capitalize(first.Key) : null,
                        SpeciesConfidence = hasSpecies ? first.Value : (double?)null,

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
