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
    /// Azure OpenAI APIと対話するためのサービス
    /// </summary>
    public class AOAIClient : IAOAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _deploymentName;
        private readonly string _apiVersion;

        private const string HttpClientName = "AzureOpenAIClient";

        /// <summary>
        /// AOAIClientクラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="httpClientFactory">HTTPクライアントファクトリ</param>
        /// <param name="configuration">Azure OpenAI設定を含む構成</param>
        public AOAIClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientName);
            
            _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? 
                throw new InvalidOperationException("AzureOpenAI:DeploymentName設定がありません");
            
            _apiVersion = configuration["AzureOpenAI:ApiVersion"] ?? "2023-05-15";
        }

        /// <summary>
        /// Azure OpenAIサービスにチャットリクエストを送信します
        /// </summary>
        /// <param name="prompt">ユーザーのメッセージ/プロンプト</param>
        /// <returns>AI応答テキスト</returns>
        public async Task<string> SendChatRequestAsync(string prompt)
        {
            try
            {
                // リクエストURLを作成（BaseAddressはHttpClientで既に設定済み）
                string requestUrl = $"openai/deployments/{_deploymentName}/chat/completions?api-version={_apiVersion}";
                
                // リクエスト本文を作成
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "あなたは役立つアシスタントです。" },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 800,
                    temperature = 0.7,
                    top_p = 0.95
                };
                
                // リクエスト本文をJSONにシリアライズ
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // Azure OpenAIにリクエストを送信
                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();
                
                // レスポンスを解析
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // レスポンス内容を抽出して返す
                if (responseObject.TryGetProperty("choices", out var choices) && 
                    choices.GetArrayLength() > 0 && 
                    choices[0].TryGetProperty("message", out var message) && 
                    message.TryGetProperty("content", out var messageContent))
                {
                    return messageContent.GetString() ?? "応答が生成されませんでした。";
                }
                
                return "応答が生成されませんでした。";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Azure OpenAI呼び出しエラー: {ex.Message}");
                return $"エラー: {ex.Message}";
            }
        }
    }
}
