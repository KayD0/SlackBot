using Microsoft.Extensions.Configuration;
using SlackBot.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlackBot.Clients
{
    /// <summary>
    /// Service for interacting with the Azure OpenAI API
    /// </summary>
    public class AOAIClient : IAOAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _deploymentName;
        private readonly string _apiVersion;

        private const string HttpClientName = "AzureOpenAIClient";

        /// <summary>
        /// Initializes a new instance of the AOAIClient class
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory</param>
        /// <param name="configuration">The configuration containing Azure OpenAI settings</param>
        public AOAIClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientName);
            
            _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? 
                throw new InvalidOperationException("AzureOpenAI:DeploymentName configuration is missing");
            
            _apiVersion = configuration["AzureOpenAI:ApiVersion"] ?? "2023-05-15";
        }

        /// <summary>
        /// Sends a chat request to Azure OpenAI service
        /// </summary>
        /// <param name="prompt">The user's message/prompt</param>
        /// <returns>The AI response text</returns>
        public async Task<string> SendChatRequestAsync(string prompt)
        {
            try
            {
                // Create the request URL (BaseAddress is already set in the HttpClient)
                string requestUrl = $"openai/deployments/{_deploymentName}/chat/completions?api-version={_apiVersion}";
                
                // Create the request body
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful assistant." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 800,
                    temperature = 0.7,
                    top_p = 0.95
                };
                
                // Serialize the request body to JSON
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // Send the request to Azure OpenAI
                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();
                
                // Parse the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Extract and return the response content
                if (responseObject.TryGetProperty("choices", out var choices) && 
                    choices.GetArrayLength() > 0 && 
                    choices[0].TryGetProperty("message", out var message) && 
                    message.TryGetProperty("content", out var messageContent))
                {
                    return messageContent.GetString() ?? "No response generated.";
                }
                
                return "No response generated.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Azure OpenAI: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}
