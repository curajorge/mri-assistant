using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RadiologistAssistant.Models;
using RadiologistAssistant.Services;

namespace RadiologistAssistant
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            var apiKey = config.GetSection("OpenAi")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                Console.WriteLine("Please set your OpenAI API key in appsettings.json");
                return;
            }

            var imageService = new ImageService();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            var openAiService = new OpenAiService(client);

            try
            {
                var scriptDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var imagesDirectory = Path.Combine(scriptDirectory, "MRI_Images");

                var imagePaths = imageService.GetImagePaths(imagesDirectory);
                var analyses = new List<Analysis>();

                foreach (var imagePath in imagePaths)
                {
                    Console.WriteLine($"Processing {Path.GetFileName(imagePath)}...");
                    var base64Image = imageService.EncodeImageToBase64(imagePath);
                    var analysis = await openAiService.GetDiagnosisFromImage(base64Image);
                    analyses.Add(analysis);
                }

                Console.WriteLine("\nGenerating final conclusion...");
                var finalConclusion = await openAiService.GetFinalConclusion(analyses);

                Console.WriteLine("\n--- Final Conclusion ---");
                Console.WriteLine($"Summary: {finalConclusion.OverallSummary}");
                if (finalConclusion.Recommendations != null && finalConclusion.Recommendations.Any())
                {
                    Console.WriteLine("Recommendations:");
                    foreach (var recommendation in finalConclusion.Recommendations)
                    {
                        Console.WriteLine($"- {recommendation}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
