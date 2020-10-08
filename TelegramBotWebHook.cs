using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;

namespace TelegramBot
{
    public class TelegramBotWebHook
    {
        private const string menu = "🍔 Hamburger\n🍅 Gazpacho\n🥚Tortilla";
        private  string topicHostname = new Uri(Environment.GetEnvironmentVariable("TopicHostName")).Host;
        private readonly ITelegramBotClient botClient;
        private readonly IEventGridClient eGClient;
        
        public TelegramBotWebHook(ITelegramBotClient client, IEventGridClient eventGridClient){
            botClient = client;
            eGClient = eventGridClient;
        }

        [FunctionName("TelegramBotWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ExecutionContext context)
        {
           
          string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
          var update = JsonConvert.DeserializeObject<Update>(requestBody);
          await HandleResponseAsync(update);  
          return new OkResult();
        }

        private async Task HandleResponseAsync(Update update){    
            switch(update.Message?.Text){
                case "/start" : await HandleStartAsync(update.Message.Chat);
                break;

                case string message when message.Contains("/order") : await HandleOrderAsync(update.Message.Chat,message);
                break;

                default: await HandleUnexpectedMessageAsync(update.Message.Chat);
                break;
            }            
        }

        private async Task HandleOrderAsync(ChatId chat, string message){

            var order = message.Split("/order")[1];        
            await eGClient.PublishEventsAsync(topicHostname, new List<EventGridEvent>() { GetEventGridEventForOrder(order, chat.Identifier) });
            await botClient.SendTextMessageAsync(
               chatId: chat,
               text: $"Your order of {order} has been processed");
        }

        private static EventGridEvent GetEventGridEventForOrder(string order, long chatId) =>
            new EventGridEvent()
            {
                Id = chatId.ToString(),
                EventTime = DateTime.UtcNow,
                DataVersion = "v1",
                EventType = "Bot.Order.Received",
                Data = order,
                Subject = "Order"
            };

        private async Task HandleUnexpectedMessageAsync(ChatId chat){
            await botClient.SendTextMessageAsync(
               chatId: chat,
               text: "Sorry but I don't understand you");
        }

        private async Task HandleStartAsync(ChatId chat) =>
          await botClient.SendTextMessageAsync(
               chatId: chat,
               text: "To order any goodie just type /order following by the name of the dish :\n" + menu);        
    }
}
