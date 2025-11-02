using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RadiologistAssistant.Models;

namespace RadiologistAssistant.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _client;

        public OpenAiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Analysis> GetDiagnosisFromImage(string base64Image)
        {
            var messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "This MRI image is part of a workflow to assist radiologists. Analyze the image and provide a structured JSON output with the following fields: 'findings' (a list of notable features, patterns, or anomalies with a 'description' and 'confidence_level' from 0 to 1), and 'summary' (a brief summary of the findings). This is not a medical diagnosis." },
                        new { type = "image", image = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                    }
                }
            };

            var requestBody = new { model = "gpt-4o", messages, max_tokens = 600 };
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from OpenAI API: {response.StatusCode} - {responseString}");
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            var analysisJson = jsonResponse.choices[0].message.content.ToString();
            return JsonConvert.DeserializeObject<Analysis>(analysisJson);
        }

        public async Task<Conclusion> GetFinalConclusion(List<Analysis> analyses)
        {
            var diagnosesJson = JsonConvert.SerializeObject(analyses);
            var messages = new[]
            {
                new { role = "system", content = "You are a medical expert providing final conclusions based on analyses." },
                new { role = "user", content = "Based on the following JSON analyses, provide a final JSON conclusion with the following fields: 'overall_summary' (a comprehensive summary of all findings), and 'recommendations' (a list of recommended next steps, if any):\n" + diagnosesJson }
            };

            var requestBody = new { model = "gpt-4o", messages, max_tokens = 2000 };
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from OpenAI API: {response.StatusCode} - {responseString}");
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            var conclusionJson = jsonResponse.choices[0].message.content.ToString();
            return JsonConvert.DeserializeObject<Conclusion>(conclusionJson);
        }
    }
}
