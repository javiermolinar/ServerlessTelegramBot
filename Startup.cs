using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using System;

[assembly: FunctionsStartup(typeof(ServerlessTelegramBot.Startup))]
namespace ServerlessTelegramBot
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {             
            ITelegramBotClient botClient = null;
            var eventGridClient = new EventGridClient(new TopicCredentials(Environment.GetEnvironmentVariable("EventGridTopicApiKey")));          

            botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramApiKey"));

            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Webhookaddress"))){
                botClient.SetWebhookAsync($"{Environment.GetEnvironmentVariable("Webhookaddress")}/api/TelegramBotWebHook/{Environment.GetEnvironmentVariable("WebhookParameters")}").Wait();
            }                      
            builder.Services.AddSingleton<ITelegramBotClient>(botClient); 
            builder.Services.AddSingleton<IEventGridClient>(eventGridClient);          
        }
    }
}