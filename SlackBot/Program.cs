using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlackBot.Clients;
using SlackBot.Clients.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Slack API用の名前付きHttpClientを登録
builder.Services.AddHttpClient("SlackApiClient", (sp, client) => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var slackBotToken = configuration["SlackBotToken"] ?? 
        throw new InvalidOperationException("SlackBotToken configuration is missing");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {slackBotToken}");
});

// Azure OpenAI API用の名前付きHttpClientを登録
builder.Services.AddHttpClient("AzureOpenAIClient", (sp, client) => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    
    var endpoint = configuration["AzureOpenAIEndpoint"] ?? 
        throw new InvalidOperationException("AzureOpenAI:Endpoint configuration is missing");
    client.BaseAddress = new Uri(endpoint);
    
    var apiKey = configuration["AzureOpenAIApiKey"] ?? 
        throw new InvalidOperationException("AzureOpenAI:ApiKey configuration is missing");
    client.DefaultRequestHeaders.Add("api-key", apiKey);
});

// LM Studio API用の名前付きHttpClientを登録
builder.Services.AddHttpClient("LMStudioClient", (sp, client) => {
    // LM Studioは通常認証が不要なので、BaseAddressはLMClientで設定します
});

// サービスを登録
builder.Services.AddSingleton<ISlackClient, SlackClient>();
builder.Services.AddSingleton<IAOAIClient, AOAIClient>();
builder.Services.AddSingleton<ILMClient, LMClient>();

builder.Build().Run();
