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

// Register SlackClient service
builder.Services.AddSingleton<ISlackClient, SlackClient>();

builder.Build().Run();
