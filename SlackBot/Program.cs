using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlackBot.Clients;
using SlackBot.Clients.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register named HttpClient for Slack API
builder.Services.AddHttpClient("SlackApiClient", (sp, client) => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var slackBotToken = configuration["SlackBotToken"] ?? 
        throw new InvalidOperationException("SlackBotToken configuration is missing");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {slackBotToken}");
});

// Register named HttpClient for Azure OpenAI API
builder.Services.AddHttpClient("AzureOpenAIClient", (sp, client) => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    
    var endpoint = configuration["AzureOpenAI:Endpoint"] ?? 
        throw new InvalidOperationException("AzureOpenAI:Endpoint configuration is missing");
    client.BaseAddress = new Uri(endpoint);
    
    var apiKey = configuration["AzureOpenAI:ApiKey"] ?? 
        throw new InvalidOperationException("AzureOpenAI:ApiKey configuration is missing");
    client.DefaultRequestHeaders.Add("api-key", apiKey);
});

// Register services
builder.Services.AddSingleton<ISlackClient, SlackClient>();
builder.Services.AddSingleton<IAOAIClient, AOAIClient>();

builder.Build().Run();
