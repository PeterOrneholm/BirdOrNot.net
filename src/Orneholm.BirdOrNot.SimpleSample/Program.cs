using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Orneholm.BirdOrNot.SimpleSample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("BirdOrNot");
            Console.WriteLine("-----------------");
            Console.WriteLine("Url: ");

            var url = Console.ReadLine();

            var AzureComputerVisionSubscriptionKey = Environment.GetEnvironmentVariable("AzureComputerVisionSubscriptionKey");
            var AzureComputerVisionEndpoint = Environment.GetEnvironmentVariable("AzureComputerVisionEndpoint");

            var computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(AzureComputerVisionSubscriptionKey))
            {
                Endpoint = AzureComputerVisionEndpoint
            };
            
            var imageAnalysis = await computerVisionClient.AnalyzeImageAsync(url, new List<VisualFeatureTypes>
            {
                VisualFeatureTypes.Objects
            });

            var isBird = imageAnalysis.Objects.Any(x => x.ObjectProperty.Equals("bird") || CheckForBirdRecursive(x.Parent));

            Console.WriteLine(isBird ? "It is a bird." : "It is not a bird.");
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
