﻿<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	// Retrieve API key from environment variable
	var apiKey = "OPENAI_API_KEY";
	if (string.IsNullOrEmpty(apiKey))
	{
		Console.WriteLine("API key not found. Please set the OPENAI_API_KEY environment variable.");
		return;
	}

    var scriptDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

    // Get the images directory relative to the script's directory
    var imagesDirectory = Path.Combine(scriptDirectory, "MRI_Images");

    if (!Directory.Exists(imagesDirectory))
    {
        Console.WriteLine($"Images directory not found: {imagesDirectory}");
        return;
    }

    var diagnoses = new List<string>();
    var imagePaths = Directory.GetFiles(imagesDirectory);

    int maxDegreeOfParallelism = 5; // Adjust based on your API's rate limits

    using (var semaphore = new SemaphoreSlim(maxDegreeOfParallelism))
    {
        var tasks = imagePaths.Select(async imagePath =>
        {
            await semaphore.WaitAsync();
            try
            {
                var diagnosis = await GetDiagnosisFromImage(apiKey, imagePath);
                lock (diagnoses)
                {
                    diagnoses.Add(diagnosis);
                }
                Console.WriteLine($"Processed {Path.GetFileName(imagePath)}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    var finalConclusion = await GetFinalConclusion(apiKey, diagnoses);
    Console.WriteLine($"Final Conclusion:\n{finalConclusion}");
}

// Method to get diagnosis from an image using GPT-4
async Task<string> GetDiagnosisFromImage(string apiKey, string imagePath)
{
    try
    {
        // Encode image to base64
        var base64Image = EncodeImageToBase64(imagePath);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // Prepare the messages with the correct content structure
        var messages = new[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = "This MRI image is part of a workflow to assist radiologists in identifying areas of interest. Please describe any notable features or patterns present in the image, such as differences in tissue density, unusual structures, or potential anomalies. This analysis should focus on assisting a radiologist in identifying areas that may require further review. This is not a medical diagnosis but a tool to support their evaluation." },
                    new
                    {
                        type = "image",
                        image = new
                        {
                            url = $"data:image/jpeg;base64,{base64Image}"
                        }
                    }
                }
            }
        };

        // Create the request body
        var requestBody = new
        {
            model = "gpt-4",
            messages = messages,
            max_tokens = 600
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        // Log the response status
        Console.WriteLine($"API Response Status ({Path.GetFileName(imagePath)}): {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            Console.WriteLine("Response content:");
            Console.WriteLine(responseString);
            return $"Error processing {Path.GetFileName(imagePath)}: {response.StatusCode}";
        }

        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
        if (jsonResponse?.choices == null || jsonResponse.choices.Count == 0)
        {
            Console.WriteLine("No choices found in the response.");
            return $"No diagnosis generated for {Path.GetFileName(imagePath)}.";
        }

        return jsonResponse.choices[0].message.content.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception occurred while processing {Path.GetFileName(imagePath)}: {ex.Message}");
        return $"An error occurred while processing {Path.GetFileName(imagePath)}.";
    }
}

// Method to encode image to base64
string EncodeImageToBase64(string imagePath)
{
    var imageBytes = File.ReadAllBytes(imagePath);
    return Convert.ToBase64String(imageBytes);
}

// Method to get final conclusion from diagnoses using GPT-4
async Task<string> GetFinalConclusion(string apiKey, List<string> diagnoses)
{
    try
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var messages = new[]
        {
            new { role = "system", content = "You are a medical expert providing final conclusions based on analyses." },
            new { role = "user", content = "Based on the following analyses, provide a final conclusion:\n" + string.Join("\n\n", diagnoses) }
        };

        var requestBody = new
        {
            model = "gpt-4",
            messages = messages,
            max_tokens = 2000
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        // Log the response status
        Console.WriteLine($"API Response Status (Final Conclusion): {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            Console.WriteLine("Response content:");
            Console.WriteLine(responseString);
            return $"Error generating final conclusion: {response.StatusCode}";
        }

		dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
		if (jsonResponse?.choices == null || jsonResponse.choices.Count == 0)
		{
			Console.WriteLine("No choices found in the response.");
			return "No final conclusion generated.";
		}

		return jsonResponse.choices[0].message.content.ToString();
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Exception occurred while generating final conclusion: {ex.Message}");
		return "An error occurred while generating the final conclusion.";
	}
}
