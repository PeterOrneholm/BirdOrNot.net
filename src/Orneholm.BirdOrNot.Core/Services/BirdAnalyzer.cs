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

            var tags = new List<string>();
            tags.AddRange(analyzedImage.Tags?.OrderByDescending(x => x.Confidence).Select(x => x.Name + (string.IsNullOrWhiteSpace(x.Hint) ? "" : $" ({x.Hint})")).ToList() ?? new List<string>());
            tags.AddRange(analyzedImage.Description?.Tags ?? new List<string>());

            var birdAnalysisMetadata = new BirdAnalysisMetadata
            {
                ImageDescription = imageDescription,
                ImageTags = tags.Distinct().ToList(),

                ImageWidth = analyzedImage.Metadata.Width,
                ImageHeight = analyzedImage.Metadata.Height,

                ImageFormat = analyzedImage.Metadata.Format
            };

            UpdateRectangles(animals, birdAnalysisMetadata);

            var isBird = GetIsBird(analyzedImage, animals);
            var animalGroups = GetAnimalGroups(analyzedImage, animals);

            if (animals.Count == 1 && string.IsNullOrWhiteSpace(animals.First().AnimalGroup))
            {
                if (animalGroups.Any())
                {
                    animals.First().AnimalGroup = animalGroups.First().Key;
                    animals.First().AnimalGroupConfidence = animalGroups.First().Value;
                }
            }

            var result = new BirdAnalysisResult
            {
                IsBird = isBird.Key,
                IsBirdConfidence = isBird.Value,

                Animals = animals,
                AnimalGroups = animalGroups.Keys.Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),

                Metadata = birdAnalysisMetadata
            };

            result.IsBirdText = GetIsBirdText(result);

            return result;
        }

        private static KeyValuePair<bool, double?> GetIsBird(ImageAnalysis analyzedImage, List<BirdAnalysisAnimal> animals)
        {
            var fromObjects = new KeyValuePair<bool, double?>(
                animals.Any(x => x.IsBird),
                animals.Where(x => x.IsBird).Max(x => x.IsBirdConfidence)
            );

            var birdTag = analyzedImage.Tags.FirstOrDefault(x => TextEqualsBird(x.Name) || TextEqualsBird(x.Hint));
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

        private static Dictionary<string, double> GetAnimalGroups(ImageAnalysis analyzedImage, List<BirdAnalysisAnimal> animals)
        {
            var animalGroups = new List<KeyValuePair<string, double>>();

            foreach (var animal in animals.Where(x => x.AnimalGroup != null))
            {
                animalGroups.Add(new KeyValuePair<string, double>(animal.AnimalGroup, animal.AnimalGroupConfidence.GetValueOrDefault(0.0)));
            }

            if(analyzedImage.Tags != null) { 
                foreach (var tag in analyzedImage.Tags.Where(x => TextEqualsBird(x.Hint)))
                {
                    animalGroups.Add(new KeyValuePair<string, double>(tag.Name, tag.Confidence));
                }
            }

            animalGroups = animalGroups.Select(x => new KeyValuePair<string, double>(Capitalize(x.Key), x.Value)).ToList();
            var result = animalGroups.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Max(y => y.Value));

            return result;
        }

        private static string GetIsBirdText(BirdAnalysisResult birdAnalysisResult)
        {
            if (birdAnalysisResult.IsBird)
            {
                if (birdAnalysisResult.HasAnimalGroup)
                {
                    return $"It's a bird ({string.Join(", ", birdAnalysisResult.AnimalGroups)})!";
                }

                return "It's a bird!";
            }

            return "It's not a bird.";
        }

        private static bool TextEqualsBird(string x)
        {
            return x != null && x.Equals(BirdObjectKey, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void UpdateRectangles(List<BirdAnalysisAnimal> birds, BirdAnalysisMetadata birdAnalysisMetadata)
        {
            foreach (var bird in birds)
            {
                var rectangle = bird.Rectangle;

                rectangle.xPercentage = GetPercentage(rectangle.x, birdAnalysisMetadata.ImageWidth);
                rectangle.yPercentage = GetPercentage(rectangle.y, birdAnalysisMetadata.ImageHeight);

                rectangle.widthPercentage = GetPercentage(rectangle.width, birdAnalysisMetadata.ImageWidth);
                rectangle.heightPercentage = GetPercentage(rectangle.height, birdAnalysisMetadata.ImageHeight);
            }
        }

        public static double GetPercentage(int value, int fullValue)
        {
            var percentage = (double)value / fullValue;
            return TruncateDecimal(percentage, 5);
        }

        public static double TruncateDecimal(double value, int precision)
        {
            var step = Math.Pow(10, precision);
            return Math.Truncate(step * value) / step;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpper(value[0]) + value.Substring(1);
        }

        private static string MakeSentence(string? value)
        {
            if (value == null || string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
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
                    var hasAnimalGroup = x.Value.Count >= 3;

                    var isAnimal = x.Value.ContainsKey(AnimalObjectKey);
                    var isBird = x.Value.ContainsKey(BirdObjectKey);

                    return new BirdAnalysisAnimal
                    {
                        AnimalGroup = hasAnimalGroup ? Capitalize(first.Key) : string.Empty,
                        AnimalGroupConfidence = hasAnimalGroup ? first.Value : (double?)null,

                        IsAnimal = isAnimal,
                        IsAnimalConfidence = isAnimal ? x.Value[AnimalObjectKey] : (double?)null,

                        IsBird = isBird,
                        IsBirdConfidence = isBird ? x.Value[BirdObjectKey] : (double?)null,

                        Rectangle = new Models.BoundingRect
                        {
                            x = x.Key.Rectangle.X,
                            y = x.Key.Rectangle.Y,
                            width = x.Key.Rectangle.W,
                            height = x.Key.Rectangle.H
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
