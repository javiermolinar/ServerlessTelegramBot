// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Telegram.Bot;
using Microsoft.Azure.EventGrid;
using System.Threading.Tasks;
using System.Threading;

namespace ServerlessTelegramBot
{
    public class ServerlessTelegramBotOrderManagement
    {
        private readonly ITelegramBotClient botClient;
        private readonly IEventGridClient eGClient;

        public ServerlessTelegramBotOrderManagement( ITelegramBotClient client, IEventGridClient eventGridClient){
            botClient = client;
            eGClient = eventGridClient;
        }

        [FunctionName("ServerlessTelegramBotOrderManagement")]
        public async Task Run([EventGridTrigger]EventGridEvent eventGridEvent)
        {
               await botClient.SendTextMessageAsync(
               chatId: new Telegram.Bot.Types.ChatId(eventGridEvent.Id),
               text: $"Your order of {eventGridEvent.Data} is in the kitchen");

               Thread.Sleep(5000);

               await botClient.SendTextMessageAsync(
               chatId: new Telegram.Bot.Types.ChatId(eventGridEvent.Id),
               text: $"Your order of {eventGridEvent.Data} is on it's way");               
        }
    }
}
