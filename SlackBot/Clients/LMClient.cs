using Microsoft.Extensions.Configuration;
using SlackBot.Clients.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlackBot.Clients
{
    /// <summary>
    /// LM Studioと対話するためのサービス
    /// </summary>
    public class LMClient : ILMClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _model;

        private const string HttpClientName = "LMStudioClient";

        /// <summary>
        /// LMClientクラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="httpClientFactory">HTTPクライアントファクトリ</param>
        /// <param name="configuration">LM Studio設定を含む構成</param>
        public LMClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientName);
            
            _baseUrl = configuration["LMStudioBaseUrl"] ?? 
                throw new InvalidOperationException("LMStudio:BaseUrl設定がありません");
            
            _model = configuration["LMStudioModel"] ?? "default";
            
            // BaseURLが設定されていない場合はデフォルト値を使用
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = "http://localhost:1234/v1";
            }
            
            // HttpClientのBaseAddressを設定
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        /// <summary>
        /// LM Studioにチャットリクエストを送信します
        /// </summary>
        /// <param name="prompt">ユーザーのメッセージ/プロンプト</param>
        /// <returns>AI応答テキスト</returns>
        public async Task<string> SendChatRequestAsync(string prompt)
        {
            try
            {
                // リクエストURLを作成
                string requestUrl = "chat/completions";
                
                // リクエスト本文を作成
                var requestBody = new
                {
                    model = _model,
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
                
                // LM Studioにリクエストを送信
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
                Console.WriteLine($"LM Studio呼び出しエラー: {ex.Message}");
                return $"エラー: {ex.Message}";
            }
        }
    }
}
