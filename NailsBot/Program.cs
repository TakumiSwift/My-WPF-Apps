using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsBot
{
    public class Program
    {

        public static event Action Timer;

        /// <summary>
        /// Метод ведения Лог-журнала бота по сообщениям юзеров
        /// </summary>
        /// <returns></returns>
        public static async Task WriteLog()
        {
            string date = DateTime.Now.ToString("dd-MM-yy");
            string path = @"D:\Visual Studio\Projects\AppsWPF\NailsBot\logs\" + date + ".txt";
            if (System.IO.File.Exists(path))
            {
                if (Bot.currentUpd.Type == UpdateType.Message)
                {
                    using (var fileStream = System.IO.File.AppendText(path))
                    {
                        fileStream.Write("текст сообщения: \""+
                                         Bot.currentUpd.Message.Text+
                                         "\" id юзера: "+
                                         Bot.currentUpd.Message.Chat.Id +
                                         $" последний ответ бота: \"{Bot.lastMsg.Text?.ToString()}\"");
                        fileStream.WriteLine(DateTime.Now.ToString("\tHH:mm:ss"));
                    }
                }
            }
            else
            {
                System.IO.File.Create(path).Close();
                if (Bot.currentUpd.Type == UpdateType.Message)
                {
                    using (var fileStream = System.IO.File.AppendText(path))
                    {
                        fileStream.Write("текст сообщения: \"" +
                                         Bot.currentUpd.Message.Text +
                                         "\" id юзера: " +
                                         Bot.currentUpd.Message.Chat.Id +
                                         $" последний ответ бота: \"{Bot.lastMsg.Text?.ToString()}\"");
                        fileStream.WriteLine(DateTime.Now.ToString("\tHH:mm:ss"));
                    }
                }
            }
        }

        /// <summary>
        /// Метод ведения Лог-журнала бота по встроенным событиям
        /// </summary>
        /// <param name="time">Время события</param>
        /// <returns></returns>
        public static async Task WriteLog(DateTime time)
        {
            string date = DateTime.Now.ToString("dd-MM-yy");
            string path = @"D:\Visual Studio\Projects\AppsWPF\NailsBot\logs\" + date + ".txt";
            if (time.Hour == 12)
            {
                using (var fileStream = System.IO.File.AppendText(path))
                {
                    fileStream.Write("Произошли события Уведомления клиентов о записях и Автоудаления окошек");
                    fileStream.WriteLine(DateTime.Now.ToString("\tHH:mm:ss"));
                }
            }
            else if(time.Hour == 18)
            {
                using (var fileStream = System.IO.File.AppendText(path))
                {
                    fileStream.Write("Произошли события Отправки карт, "+
                                     "Удаления записей и Автоудаления окошек");
                    fileStream.WriteLine(DateTime.Now.ToString("\tHH:mm:ss"));
                }
            }
        }

        static async Task Main(string[] args)
        {
            Bot botClient = new();
            await Bot.InitializeBotCommands();
            
            while(true)
            {
                Timer();
                await Task.Delay(new TimeSpan(0, 1, 0));
            }
        }
    }
}
