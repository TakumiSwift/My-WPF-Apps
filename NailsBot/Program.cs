using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace NailsBot
{
    public class Program
    {

        public static event Action Timer;

        static async Task Main(string[] args)
        {
            Bot botClient = new();
            await Bot.InitializeBotCommands();
            
            while(true)
            {
                Timer();
                await Task.Delay(new TimeSpan(0, 0, 45));
            }
        }
    }
}
