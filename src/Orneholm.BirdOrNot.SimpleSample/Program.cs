using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace Orneholm.BirdOrNot.SimpleSample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("BirdOrNot");
            Console.WriteLine("-----------------");
            Console.WriteLine();

            var azureComputerVisionSubscriptionKey = Environment.GetEnvironmentVariable("AzureComputerVisionSubscriptionKey");
            var azureComputerVisionEndpoint = Environment.GetEnvironmentVariable("AzureComputerVisionEndpoint");

            var computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(azureComputerVisionSubscriptionKey))
            {
                Endpoint = azureComputerVisionEndpoint
            };

            while (true)
            {
                Console.WriteLine("Url: ");
                var url = Console.ReadLine();
                var imageAnalysis = await computerVisionClient.AnalyzeImageAsync(url, new List<VisualFeatureTypes>
                {
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Description
                });

                var isBird = imageAnalysis.Objects.Any(x => x.ObjectProperty.Equals("bird") || CheckForBirdRecursive(x.Parent));

                Console.WriteLine(isBird ? "It is a bird!" : "It is not a bird.");
                Console.WriteLine("Description: " + imageAnalysis.Description.Captions.FirstOrDefault()?.Text);

                Console.WriteLine();

                var json = JsonConvert.SerializeObject(imageAnalysis, Formatting.Indented);

                Console.WriteLine("Full result:");
                Console.WriteLine("------------------------");
                Console.WriteLine(json);
                Console.WriteLine("------------------------");

                Console.WriteLine();
            }
        }

        private static bool CheckForBirdRecursive(ObjectHierarchy detectedObject)
        {
            if (detectedObject == null)
            {
                return false;
            }

            if (detectedObject.ObjectProperty.Equals("bird"))
            {
                return true;
            }

            return detectedObject.Parent != null && CheckForBirdRecursive(detectedObject.Parent);
        }
    }
}
