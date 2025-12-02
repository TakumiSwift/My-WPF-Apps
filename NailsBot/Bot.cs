using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailsBot
{
    public static class Ext
    {
        public static async Task<Message> SendMsg(this ITelegramBotClient bot, ChatId chatId, string Msg)
        {
            return await bot.SendTextMessageAsync(chatId, Msg);
        }
        public static async Task<Message> SendMsg(this ITelegramBotClient bot,
                                                  ChatId chatId, 
                                                  string Msg, 
                                                  ParseMode parsemode)
        {
            return await bot.SendTextMessageAsync(chatId, Msg);
        }
    }

    public class Bot
    {
        /// <summary>
        /// Событие для уведомления клиентов об их записи
        /// </summary>
        public static event Func<DateTime, Dictionary<string, Client>, Task> CurrentDate;

        /// <summary>
        /// Событие для автоудаления неактуальных окошек
        /// </summary>
        public static event Func<DateTime, Task> WindowDate;

        /// <summary>
        /// Экземпляр клиента бота
        /// </summary>
        public static ITelegramBotClient botClient;

        /// <summary>
        /// Экземпляр класса с данными
        /// </summary>
        public static Data data;

        /// <summary>
        /// Временная коллекция данных для записи
        /// </summary>
        public static List<string> note;

        /// <summary>
        /// Лог. переменная наличия фото в записи
        /// </summary>
        public static bool flag1;

        /// <summary>
        /// Лог. переменная выбранной корректировки
        /// </summary>
        public static bool flag2;

        /// <summary>
        /// Лог. переменная просмотра записей в выбранный день
        /// </summary>
        public static bool flag3;

        /// <summary>
        /// Последнее отправленное ботом сообщение
        /// </summary>
        public static Message lastMsg;

        /// <summary>
        /// Последнее отправленное пользователем сообщение
        /// </summary>
        public static Message userMsg;

        /// <summary>
        /// Коллекция записанных ранее пользователей
        /// </summary>
        public static Dictionary<long,Client> users;

        /// <summary>
        /// Текущий Update
        /// </summary>
        public static Update currentUpd;

        public static Dictionary<string, List<string>> dates;

        public static List<string> times;

        public static int daysCount;

        public static string result;

        /// <summary>
        /// Инициализация и включение бота
        /// </summary>
        public Bot()
        {
            data = new Data();
            users = new();
            flag1 = true;
            flag2 = true;
            flag3 = true;
            botClient = new TelegramBotClient("8497096632:AAF2FEWHxFiv8iy76fkUr94qSygwHTDwx0M");
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions
            );
            CurrentDate += BotDialogLogic.NoteConfirm;
            WindowDate += BotDialogLogic.WindowDelete;
            Program.Timer += TimeCheck;            
            lastMsg = new();
            userMsg = new();
            dates = new();
            times = new();
            daysCount = 0;
            result = "";
        }

        /// <summary>
        /// Метод, задающий реакцию на все обновления пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient,
                                                   Update update, 
                                                   CancellationToken cancellationToken)
        {
            currentUpd = update;
            await Program.WriteLog();
            if (update.Type != UpdateType.Message)
                return;
            if (data.ContainClient(Convert.ToString(update.Message.From.Id)))
            {
                data.GetClientById(Convert.ToString(update.Message.From.Id), out users);
            }
            await RulesCheck(botClient, update);
            var message = update.Message;
            var chatId = update.Message.Chat.Id;
            var userId = update.Message.From.Id;
            var msgText = message.Text;
            await UserStateCheck(msgText, chatId, message, userId, update);
            CommandCheck(msgText, chatId, message, userId, update);
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient,
                                                  Exception exception,
                                                  CancellationToken cancellationToken)
        {
            string date = DateTime.Now.ToString("dd-MM-yy");
            string path = @"D:\Visual Studio\Projects\AppsWPF\NailsBot\logs\" + date + ".txt";
            Console.WriteLine($"Произошла ошибка: {exception.Message} В {exception.TargetSite}");
            using (var fileStream = System.IO.File.AppendText(path))
            {
                fileStream.Write("текст сообщения: \"" +
                                 currentUpd.Message.Text?.ToString() +
                                 "\" id юзера: " +
                                 currentUpd.Message.Chat.Id +
                                 $" последний ответ бота: \"{lastMsg.Text?.ToString()}\" " +
                                 $"Произошла ошибка: В {exception.TargetSite} "+
                                 $"вызвал ошибку {exception.Source} Описание: {exception.Message}");
                fileStream.WriteLine(DateTime.Now.ToString("\tHH:mm:ss"));
            }
        }

        /// <summary>
        /// Инициализация команд при запуске бота
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeBotCommands()
        {
            var defaultCommands = new List<BotCommand>
            {
                new() { Command = "/addnewnote", Description = "💌Записаться" },
                new() { Command = "/cancelmynote", Description = "❌Отменить запись" },
                new() { Command = "/price", Description = "💸Прайс услуг" },
                new() { Command = "/windows", Description = "📆Окошки" },
                new() { Command = "/location", Description = "📍Как добраться?"}
            };

            await botClient.SetMyCommandsAsync(
                commands: defaultCommands,
                scope: BotCommandScope.Default());
        }

        /// <summary>
        /// Проверка прав пользователя по его id
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task RulesCheck(ITelegramBotClient botClient, Update upd)
        {
            if (upd.Message?.From == null) return;

            var userId = upd.Message.From.Id;
            var chatId = upd.Message.Chat.Id;
            
            if (data.IsAdmin(userId))
            {
                // Команды для администратора
                var adminCommands = new List<BotCommand>
                {
                    new() { Command = "/shownotes", Description = "Показать все записи" },
                    new() { Command = "/shownotesbyday", Description = "Показать записи на определенный день" },
                    new() { Command = "/addnewmonth", Description = "Создать окошки на следующий месяц" },
                    new() { Command = "/takewindow", Description = "Убрать окошки, занятые вне бота" }
                };

                await botClient.SetMyCommandsAsync(
                    commands: adminCommands,
                    scope: BotCommandScope.Chat(chatId));
            }
            else
            {
                // Команды для обычных пользователей
                var defaultCommands = new List<BotCommand>
                {
                    new() { Command = "/addnewnote", Description = "💌Записаться" },
                    new() { Command = "/cancelmynote", Description = "❌Отменить запись" },
                    new() { Command = "/price", Description = "💸Прайс услуг" },
                    new() { Command = "/windows", Description = "📆Окошки" },
                    new() { Command = "/location", Description = "📍Как добраться?"}
                };

                if (users.ContainsKey(upd.Message.From.Id))
                {
                    defaultCommands.RemoveAt(0);
                    defaultCommands.Insert( 0, new BotCommand() { Command = "/mynote", Description = "📝Моя запись" });
                }
                await botClient.SetMyCommandsAsync(
                    commands: defaultCommands,
                    scope: BotCommandScope.Chat(chatId)); // Исправлено!
            }
        }

        /// <summary>
        /// Метод проверки введенной команды
        /// </summary>
        /// <param name="msgText"></param>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="userId"></param>
        /// <param name="update"></param>
        private static async void CommandCheck(string msgText,
                                               long chatId,
                                               Message message,
                                               long userId, 
                                               Update update)
        {
            switch (msgText)
            {
                case "/mynote":
                    await BotDialogLogic.GetMyNote(botClient, update, data, users[message.From.Id]);
                    break;
                case "/windows":
                    await BotDialogLogic.GetWindows(botClient, chatId, data);
                    break;
                case "/addnewnote":
                    await BotDialogLogic.StartAddNoteDialog(botClient, userId, chatId);
                    break;
                case "/price":
                    await BotDialogLogic.GetPrice(botClient, chatId, data);
                    break;
                case "/cancelmynote":
                    await BotDialogLogic.CancelNote(botClient, update, data);
                    break;
                case "/location":
                    await BotDialogLogic.GetLocation(botClient, update, data);
                    break;
                case "/shownotes":
                    if (data.IsAdmin(userId))
                    {
                        await BotDialogLogic.GetClients(data.GetAllClients(), botClient, update);
                    }
                    break;
                case "/shownotesbyday":
                    if (data.IsAdmin(userId))
                    {
                        await BotDialogLogic.GetClientsByDay("0", chatId);
                        UserStateManager.UserStates.Add(userId, new UserState());
                        userMsg = message;
                        flag3 = false;
                    }
                    break;
                case "/addnewmonth":
                    if (data.IsAdmin(userId))
                    {
                        UserStateManager.UserStates.Add(userId, new UserState());
                        UserStateManager.UserStates[userId].CurrentCommand = "/addnewmonth";
                        UserStateManager.UserStates[userId].Step = 0;
                        await BotDialogLogic.AddNewMonth(botClient, update);
                    }
                    break;
                case "/takewindow":
                    if (data.IsAdmin(userId))
                    {
                        UserStateManager.UserStates.Add(userId, new UserState());
                        UserStateManager.UserStates[userId].CurrentCommand = "/takewindow";
                        UserStateManager.UserStates[userId].Step = 0;
                        await BotDialogLogic.TakeWindow(botClient, update);
                    }
                    break;
            }
        }

        /// <summary>
        /// Метод проверки активного диалога у пользователя
        /// </summary>
        /// <param name="msgText"></param>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="userId"></param>
        /// <param name="update"></param>
        private static async Task UserStateCheck(string msgText,
                                               long chatId,
                                               Message message,
                                               long userId,
                                               Update update)
        {
            bool[] flags = 
            { 
                msgText == "/price", 
                msgText == "/windows",
                msgText == "/location",
                msgText == "/cancelmynote"
            };
            if (UserStateManager.UserStates.ContainsKey(userId))
            {
                if (flags[0] || flags[1] || flags[2] || flags[3])
                {
                    return;
                }
                else
                {
                    if (userMsg.Text == "/shownotesbyday")
                    {
                        await BotDialogLogic.GetClientsByDay(msgText, chatId);
                        UserStateManager.UserStates.Remove(userId);
                    }
                    else
                    {
                        if (UserStateManager.UserStates[userId].CurrentCommand == "/addnewmonth")
                        {
                            await BotDialogLogic.AddNewMonth(botClient, update);
                        }
                        else if (UserStateManager.UserStates[userId].CurrentCommand == "/addnewnote")
                        {
                            if (msgText == "/addnewnote")
                            {
                                UserStateManager.UserStates.Remove(userId);
                                Message msg = await Ext.SendMsg(botClient, 
                                                                    chatId, 
                                                                    "Создание записи будет перезапущено!");
                                await Task.Delay(2000);
                                await botClient.DeleteMessageAsync(chatId, msg.MessageId);
                            }
                            else
                            {
                                await BotDialogLogic.ContinueAddNoteDialog(botClient,
                                                                                  userId,
                                                                                  chatId,
                                                                                  msgText,
                                                                                  update,
                                                                                  data);
                            }
                        }
                        else if (UserStateManager.UserStates[userId].CurrentCommand == "/takewindow")
                        {
                            await BotDialogLogic.TakeWindow(botClient, update);
                        }
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Метод запуска события проверки напоминания о записи
        /// </summary>
        /// <param name="user"></param>
        private static async void TimeCheck()
        {
            if (DateTime.Now.Hour == 12 && (DateTime.Now.Minute == 00 || DateTime.Now.Minute == 01))
            {
                await CurrentDate(DateTime.Now, data.GetAllClients());
                await WindowDate(DateTime.Now);
                await Program.WriteLog(DateTime.Now);
            }
            if (DateTime.Now.Hour == 18 && (DateTime.Now.Minute == 00 || DateTime.Now.Minute == 01))
            {
                await BotDialogLogic.ClientCard(botClient, currentUpd);
                await Task.Delay(50);
                await BotDialogLogic.DeleteNotesAuto(botClient, currentUpd);
                await WindowDate(DateTime.Now);
                await Program.WriteLog(DateTime.Now);
            }
        }

    }
}
