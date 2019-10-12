﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using Orneholm.BirdOrNot.Web.Models;

namespace Orneholm.BirdOrNot.Web.Services
{
    public class BirdAnalyzer : IBirdAnalyzer
    {
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

            return new BirdAnalysisResult
            {
                IsBird = birds.Any(),
                IsBirdConfidence = birds.Max(x => x.IsBirdConfidence),
                Birds = birds,
                Metadata = new BirdAnalysisMetadata
                {
                    ImageDescription = analyzedImage.Description?.Captions?.FirstOrDefault()?.Text,
                    ImageTags = analyzedImage.Description?.Tags.ToList()
                }
            };
        }

        private static IEnumerable<BirdAnalysisBird> GetBirds(ImageAnalysis analyzedImage)
        {
            return analyzedImage.Objects.ToDictionary(x => x, GetObjectHierarchy)
                .Where(x => x.Value.ContainsKey("bird"))
                .Select(x =>
                {
                    var first = x.Value.FirstOrDefault();
                    var firstIsBird = first.Key == "bird";

                    return new BirdAnalysisBird
                    {
                        BirdSpiecies = !firstIsBird ? first.Key : null,
                        IsBirdSpieciesConfidence = !firstIsBird ? first.Value : (double?)null,
                        IsAnimalConfidence = x.Value["animal"],
                        IsBirdConfidence = x.Value["bird"],
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
