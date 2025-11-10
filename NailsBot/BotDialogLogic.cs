using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailsBot
{
    public static class BotDialogLogic
    {
        #region Группа методов по созданию новой записи

        /// <summary>
        /// Метод начала создания новой записи
        /// </summary>
        /// <param name="botClient">текущий клиент-бот ТГ</param>
        /// <param name="userId">id пользователя</param>
        /// <param name="chatId">id чата</param>
        /// <returns></returns>
        public static async Task StartAddNoteDialog(ITelegramBotClient botClient, long userId, long chatId)
        {
            // Создаем состояние для пользователя
            UserStateManager.UserStates[userId] = new UserState
            {
                CurrentCommand = "/addnewnote",
                Step = 1
            };
            Message msg = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Обратите внимание!\n\n" +
                      "Если вы введете команду /addnewnote, придется формировать запись сначала!\n"
            );
            await Task.Delay(3000);
            await botClient.DeleteMessageAsync(chatId, msg.MessageId);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "✨Давайте начнем!\n\nКак Вас зовут?"
            );
        }

        /// <summary>
        /// Метод создания новой записи
        /// </summary>
        /// <param name="botClient">текущий клиент-бот ТГ</param>
        /// <param name="userId">id пользователя</param>
        /// <param name="chatId">id чата</param>
        /// <param name="messageText">текст текущего сообщения пользователя</param>
        /// <param name="upd">экземпляр текущего обновления</param>
        /// <returns></returns>
        public static async Task ContinueAddNoteDialog(ITelegramBotClient botClient,
                                                        long userId, 
                                                        long chatId, 
                                                        string messageText, 
                                                        Update upd,
                                                        Data data)
        {
            var userState = UserStateManager.UserStates[userId];

            switch (userState.Step)
            {
                case 1: // Шаг 1: Имя клиента
                    userState.ClientName = messageText;
                    userState.Step = 2;

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Приятно познакомиться!\n\n📱 Как с вами связаться?\n\n" +
                              $"Пожалуйста, укажите:\n" +
                              $"▫️Ваш номер телефона, если он не скрыт\n" +
                              $"▫️Или Ваш @username в Telegram"
                    );
                    break;

                case 2: // Шаг 2: Номер телефона
                    userState.Phone = messageText;
                    userState.Step = 3;

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Записала!\n💅Теперь укажите: на какую услугу вы хотите записаться?\n\n" +
                              "Посмотреть список оказываемых услуг вы можете, прописав команду\n" +
                              "\"/price\"\n\n" +
                              "Пример описания желаемой услуги:\n\"Наращивание до 5 длины\""
                    );
                    break;

                case 3: // Шаг 3: Название услуги
                    userState.Service = messageText;
                    userState.Step = 4;

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Прекрасный выбор!\n🗓️Теперь выберите окошко для записи:\n\n" +
                              "Когда подберете удобные Вам дату и время\n" +
                              "Пожалуйста, напишите их в формате \"01.01  10:00\""
                    );
                    await GetWindows(botClient, chatId, data);
                    break;
                case 4: // Шаг 4: Дата окошка
                    try
                    {
                        userState.Date = messageText;
                        if (!Bot.data.TryFindWindow(userState.Date)) throw new Exception("Нет такой Даты/Времени");
                        string[] line = messageText.Split(' ');
                        userState.Step = 5;

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Увидимся {line[0]} в {line[1]}!\n" +
                                  $"🎨Есть ли у вас пожелания к маникюру?\n\n" +
                                  $"Если хотите, можете:\n" +
                                  $"▫️Прислать фото-референс или ссылку на него\n" +
                                  $"▫️Описать цвет и дизайн словами\n" +
                                  $"▫️Указать особые пожелания\n\n" +
                                  $"Или просто напишите \"без пожеланий\""
                        );
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(chatId, "Произошла ошибка ввода, попробуйте еще раз!\n"+
                                                                     "Пожалуйста, напишите \nВыбранные дату и время "+
                                                                     "из списка окошек\nВ формате \"01.01  10:00\"");
                        userState.Step = 4;
                    }
                    break;
                case 5: // Шаг 5: Референс
                    userState.Step = 6;
                    try
                    {
                        if (upd.Message.Photo == null)
                        {
                            userState.Reference = messageText;
                            await ContinueAddNoteDialog(botClient, userId, chatId, messageText, upd, data);
                        }
                        else
                        {
                            DownloadUserPhoto(botClient, upd.Message, chatId, userId);
                            await Task.Delay(3000);
                            await ContinueAddNoteDialog(botClient, userId, chatId, messageText, upd, data);
                        }
                    }
                    catch
                    {
                        userState.Step = 5;
                        await Ext.SendMsg(botClient, chatId, "Произошла ошибка, попробуйте еще раз!");
                    }
                    break;
                case 6: // Шаг 6 Проверка введенных данных
                    userState.Step = 7;
                    await botClient.SendTextMessageAsync(
                          chatId: chatId,
                          text: "Хорошо!\nТеперь, пожалуйста " +
                                "проверьте вашу запись перед отправкой:\n\n" +
                                $"\U0001faaa Имя: <i><b>{userState.ClientName}</b></i>\n" +
                                $"📞 Телефон/@username: <i><b>{userState.Phone}</b></i>\n" +
                                $"💅 Услуга: <i><b>{userState.Service}</b></i>\n" +
                                $"📆 Дата и время: <i><b>{userState.Date}</b></i>\n\n" +
                                "Если все указано верно, напишите \"Да\",\n" +
                                "Если нужно внести изменения в каком-либо из пунктов,\n" +
                                "То введите название этого пункта.",
                          parseMode: ParseMode.Html);
                    break;
                case 7: // Шаг 7 Корректировка и отправка записи
                    if (Bot.flag2) await CorrectNoteChoice(botClient, chatId, upd.Message, userState, upd);
                    else await CorrectingNote(botClient, chatId, upd.Message, userState);
                    break;
            }
        }

        /// <summary>
        /// Метод завершения создания записи
        /// </summary>
        /// <param name="botClient">текущий клиент-бот ТГ</param>
        /// <param name="userId">id пользователя</param>
        /// <param name="chatId">id чата</param>
        /// <param name="userState">экземпляр текущего взаимодействия с пользователем</param>
        /// <returns></returns>
        public static async Task EndAddNoteDialog(ITelegramBotClient botClient,
                                                   long userId, 
                                                   long chatId, 
                                                   UserState userState, 
                                                   bool flag)
        {
            try
            {
                // Сохраняем услугу в XML
                Bot.note = new List<string>
                {
                    Convert.ToString(userId),
                    UserStateManager.UserStates[userId].ClientName,
                    UserStateManager.UserStates[userId].Phone,
                    UserStateManager.UserStates[userId].Service,
                    UserStateManager.UserStates[userId].Date,
                    UserStateManager.UserStates[userId].Reference,
                    Convert.ToString(Bot.currentUpd.Message.Chat.Id)
                };

                // Ваш метод сохранения
                if (flag) Bot.data.AddNote(Bot.note);
                else Bot.data.ReplaceNote(Bot.note);
                Bot.data.TakeWindow(UserStateManager.UserStates[userId].Date);
                // Удаляем состояние пользователя
                UserStateManager.UserStates.Remove(userId);
                Bot.note.Clear();

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"📝 Запись успешно создана!\n\n" +
                          $"Буду ждать Вас по адресу:\n" +
                          $"г.Волгоград, ул. Невская, д.13А, этаж 8, каб. 803 🤍"
                );
                Bot.data.GetClientById(Convert.ToString(chatId), out Bot.users);
                await Bot.RulesCheck(botClient, Bot.currentUpd);
                for(int i = 0; i < 2; i++)
                {
                    await Ext.SendMsg(botClient, Bot.data.GetAdminId()[i], "Кто-то записался!");
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Произошла ошибка при создании записи {ex}. Попробуйте снова."
                );

                // Очищаем состояние даже при ошибке
                UserStateManager.UserStates.Remove(userId);
                Bot.note.Clear();
            }
        }

        /// <summary>
        /// Метод загрузки изображения на Desktop
        /// </summary>
        /// <param name="botClient">текущий клиент-бот ТГ</param>
        /// <param name="message">экземпляр сообщения</param>
        /// <param name="chatId">id чата</param>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
        public static async Task DownloadUserPhoto(ITelegramBotClient botClient, 
                                                   Message message, 
                                                   long chatId, 
                                                   long userId)
        {
            try
            {
                Bot.flag1 = false;
                // 1. Получаем фото с наивысшим разрешением (последний элемент в массиве)
                var photoSize = message.Photo.Last();
                var fileId = photoSize.FileId;

                // 2. Получаем информацию о файле
                var fileInfo = await botClient.GetFileAsync(fileId);
                if (fileInfo == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Не удалось получить информацию о файле от Telegram.");
                    return;
                }

                // 3. Формируем уникальное имя для файла
                var fileExtension = Path.GetExtension(fileInfo.FilePath); // Например, ".jpg"
                var uniqueFileName = $"photo_{message.From.Id}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                // 4. Получаем путь для сохранения через вашу систему данных
                var localFilePath = Path.Combine(Bot.data.GetPaths("reference"), uniqueFileName);

                // 5. Скачиваем и сохраняем файл
                await using (var fileStream = System.IO.File.OpenWrite(localFilePath))
                {
                    await botClient.DownloadFileAsync(fileInfo.FilePath, fileStream);
                }

                // Сохраняем путь к файлу в состояние пользователя
                UserStateManager.UserStates[userId].Reference = localFilePath;
                // Сохраняем услугу в XML
                Bot.note = new List<string>
                {
                    Convert.ToString(userId),
                    UserStateManager.UserStates[userId].ClientName,
                    UserStateManager.UserStates[userId].Phone,
                    UserStateManager.UserStates[userId].Service,
                    UserStateManager.UserStates[userId].Date,
                    UserStateManager.UserStates[userId].Reference,
                    Convert.ToString(Bot.currentUpd.Message.Chat.Id)
                };

                // Ваш метод сохранения
                Bot.data.AddNote(Bot.note);

            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Ошибка при загрузке фото: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Метод выбора наличия корректировок в записи
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public static async Task CorrectNoteChoice(ITelegramBotClient botClient,
                                                   long chatId,
                                                   Message message,
                                                   UserState userState,
                                                   Update upd)
        {
            string line = "";
            Bot.flag2 = false;
            switch (message.Text.ToLower())
            {
                case "да":
                    await EndAddNoteDialog(botClient, message.From.Id, chatId, userState, Bot.flag1);
                    return;
                case "имя":
                    line = "новое Имя:";
                    break;
                case "услуга":
                    line = "другую Услугу:";
                    break;
                case "услуги":
                    line = "другую Услугу:";
                    break;
                case "телефон":
                    line = "новый Номер телефона или @username:";
                    break;
                case "@username":
                    line = "@username или новый Номер телефона:";
                    break;
                case "дата":
                    line = "новые Дату и время:";
                    break;
                case "время":
                    line = "новые Время и дату:";
                    break;
                case "дата и время":
                    line = "новые Дату и время:";
                    break;
                default:
                    await Ext.SendMsg(botClient, chatId, "Указанный Вами пункт отстутствует,\n" +
                                                         "Либо вы ошиблись.\nПопробуйте еще раз!");
                    Bot.flag2 = true;
                    userState.Step = 6;
                    await ContinueAddNoteDialog(botClient,message.From.Id,chatId,message.Text,upd,Bot.data);
                    return;
            }
            Bot.lastMsg = await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: $"Введите, пожалуйста {line}");
        }

        /// <summary>
        /// Метод непосредственной корректировки записи и сохранение ее в базу
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public static async Task CorrectingNote(ITelegramBotClient botClient,
                                                long chatId,
                                                Message message,
                                                UserState userState)
        {
            string line = Bot.lastMsg.Text.Split("пожалуйста ")[1];
            switch (line)
            {
                case "новое Имя:":
                    userState.ClientName = message.Text;
                    break;
                case "другую Услугу:":
                    userState.Service = message.Text;
                    break;
                case "новый Номер телефона или @username:":
                    userState.Phone = message.Text;
                    break;
                case "@username или новый Номер телефона:":
                    userState.Phone = message.Text;
                    break;
                case "новые Дату и время:":
                    if (Bot.data.TryFindWindow(message.Text) != true)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Произошла ошибка ввода, попробуйте еще раз!\n" +
                                                                 "Пожалуйста, напишите \nВыбранные дату и время " +
                                                                 "из списка окошек\nВ формате \"01.01  10:00\"");
                        return;
                    }
                    if (Bot.flag1)
                    { 
                        userState.Date = message.Text;
                    }
                    else 
                    { 
                        Bot.data.ReturnWindow(userState.Date);
                        userState.Date = message.Text;
                    }
                    break;
                case "новые Время и дату:":
                    if (Bot.data.TryFindWindow(message.Text) != true)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Произошла ошибка ввода, попробуйте еще раз!\n" +
                                                                 "Пожалуйста, напишите \nвыбранные дату и время " +
                                                                 "из списка окошек\nв формате \"01.01  10:00\"");
                        return;
                    }
                    if (Bot.flag1) { userState.Date = message.Text; }
                    else
                    {
                        Bot.data.ReturnWindow(userState.Date);
                        userState.Date = message.Text;
                    }
                    break;
            }
            await EndAddNoteDialog(botClient, message.From.Id, chatId, userState, Bot.flag1);
        }

        #endregion


        /// <summary>
        /// Метод получени Прайса
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static async Task GetPrice(ITelegramBotClient botClient, ChatId chatId, Data data)
        {
            botClient.SendTextMessageAsync(chatId, "💸Вот актуальный прайс:");
            await using (var fileStream = new FileStream(data.GetPaths("price1"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromStream(fileStream, Path.GetFileName(data.GetPaths("price1"))),
                    parseMode: ParseMode.Html
                );
            }
            await using (var fileStream = new FileStream(data.GetPaths("price2"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromStream(fileStream, Path.GetFileName(data.GetPaths("price2"))),
                    parseMode: ParseMode.Html
                );
            }
        }

        /// <summary>
        /// Метод получения Окошек
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static async Task GetWindows(ITelegramBotClient botClient, ChatId chatId, Data data)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            Dictionary<string, List<string>> windows = data.GetWindows(Convert.ToString(month), Convert.ToString(year));
            string answer = "Доступные окошки на текущий месяц:\n〰️〰️〰️〰️〰️〰️〰️〰️〰️\n";
            foreach (var item in windows)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (i == 0) { answer += $"🎀{item.Key}.{month}     {item.Value[i]};"; }
                    else { answer += $" {item.Value[i]};"; }
                    if (i == item.Value.Count - 1) { answer += "\n\n"; }
                }
            }
            answer += "〰️〰️〰️〰️〰️〰️〰️〰️〰️";
            await Ext.SendMsg(botClient, chatId, answer);
        }

        /// <summary>
        /// Метод удаления записи
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task CancelNote(ITelegramBotClient botClient, Update upd, Data data)
        {
            int res = 1;
            data.DeleteNote(Convert.ToString(upd.Message.From.Id), out res);
            if (res == 1) { await botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Ваша запись была удалена!"); }
            else { await botClient.SendTextMessageAsync(upd.Message.Chat.Id, "У Вас не было записи!"); }
            Bot.users.Remove(upd.Message.From.Id);
            await Bot.RulesCheck(botClient,Bot.currentUpd);
        }

        /// <summary>
        /// Метод показа всех записей
        /// </summary>
        /// <param name="data"></param>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task GetClients(Dictionary<string,Client> data,
                                            ITelegramBotClient botClient, 
                                            Update upd)
        {
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    if (Data.HavePhoto(Convert.ToInt64(item.Value.Id)) == "Без фото")
                    {
                        await botClient.SendTextMessageAsync(
                                 chatId: upd.Message.Chat.Id,
                                   text: $"Имя: {item.Value.Name}\n" +
                                         $"Телефон/тг: {item.Value.Phone}\n" +
                                         $"Услуга: {item.Value.ClientNote.Service}\n" +
                                         $"Дата и время: {item.Value.ClientNote.Date}\n" +
                                         $"Референс: {item.Value.ClientNote.Reference}");
                    }
                    else
                    {
                        await botClient.SendPhotoAsync(
                                 chatId: upd.Message.Chat.Id,
                                  photo: InputFile.FromStream(System.IO.File.OpenRead(item.Value.ClientNote.Reference),
                                                              Path.GetFileName(item.Value.ClientNote.Reference)),
                                caption: $"Имя: {item.Value.Name}\n" +
                                         $"Телефон/тг: {item.Value.Phone}\n" +
                                         $"Услуга: {item.Value.ClientNote.Service}\n" +
                                         $"Дата и время: {item.Value.ClientNote.Date}\n" +
                                         $"Референс приложен",
                              parseMode: ParseMode.Html);
                    }
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                         chatId: upd.Message.Chat.Id,
                           text: "Записей нет :(");
            }
        }

        /// <summary>
        /// Метод вывода клиентов в определенный день
        /// </summary>
        /// <param name="day"></param>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static async Task GetClientsByDay(string day, long chatId)
        {
            if (Bot.flag3) await Ext.SendMsg(Bot.botClient, chatId, "Введите день в формате \"01.01\":");
            else
            {
                Dictionary<string, Client> data = new();
                foreach (var item in Bot.data.GetAllClients())
                {
                    if (day == item.Value.ClientNote.Date.Split(" ")[0])
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                await GetClients(data, Bot.botClient, Bot.currentUpd);
                Bot.flag3 = true;
            }
        }

        /// <summary>
        /// Метод получения инструкции Как добраться
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task GetLocation(ITelegramBotClient botClient, Update upd, Data data)
        {
            using (var fileStream = new FileStream(data.GetPaths("video"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var vid = InputFile.FromStream(fileStream, Path.GetFileName(data.GetPaths("video")));
                await botClient.SendVideoAsync(
                     chatId: upd.Message.Chat.Id,
                      video: vid,
                    caption: "Вот короткое видео, показывающее\n"+
                             "Как добраться до кабинета\n"+
                             "От остановки \"Родина\"");
            }
        }

        /// <summary>
        /// Метод уведомления клиентов об их записи
        /// </summary>
        /// <param name="day">текущий день</param>
        /// <param name="data">коллекция всех клиентов</param>
        /// <returns></returns>
        public static async Task NoteConfirm(DateTime day, Dictionary<string, Client> data)
        {
            foreach (var item in data)
            {
                if (day.Day + 1 == Convert.ToInt32(item.Value.ClientNote.Date.Split(" ")[0].Split(".")[0]))
                {
                    await Ext.SendMsg(Bot.botClient,
                                      item.Value.ChatId,
                                      $"🔔Напоминаю Вам о записи на завтра " +
                                      $"в {item.Value.ClientNote.Date.Split(" ")[1]}!\n"+
                                      "Если вы Не Подтверждаете свою запись, то введите "+
                                      "команду /cancelmynote\n"+
                                      "В ином случае запись будет Подтверждена!");
                }
            }
        }

        /// <summary>
        /// Метод автоудаления окошек в текущий день
        /// </summary>
        /// <param name="day">текущий день</param>
        /// <returns></returns>
        public static async Task WindowDelete(DateTime day)
        {
            Bot.data.DeleteWindows(day.Date.ToString("dd.MM.yyyy"));
        }

        /// <summary>
        /// Метод вывода записи клиента
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <param name="data"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task GetMyNote(ITelegramBotClient botClient, 
                                           Update upd, 
                                           Data data, 
                                           Client user)
        {
            try
            {
                await botClient.SendTextMessageAsync(
                    chatId: upd.Message.Chat.Id,
                    text: "📝 Ваша запись:\n\n" +
                         $"🪪 Имя, указанное в записи: <i><b>{user.Name}</b></i>\n" +
                         $"📞 Телефон/@username: <i><b>{user.Phone}</b></i>\n" +
                         $"💅 Услуга: <i><b>{user.ClientNote.Service}</b></i>\n" +
                         $"📆 Дата и время записи: <i><b>{user.ClientNote.Date}</b></i>",
                    parseMode: ParseMode.Html);
            }
            catch
            {
                await Ext.SendMsg(
                    bot: botClient,
                    chatId: upd.Message.Chat.Id,
                    Msg: "У вас нет активной записи!");
            }
        }

        /// <summary>
        /// Метод создания нового месяца с окошками
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task AddNewMonth(ITelegramBotClient botClient, 
                                             Update upd)
        {
            var userId = upd.Message.From.Id;
            var chatId = upd.Message.Chat.Id;
            var msgText = upd.Message.Text;
            var userState = UserStateManager.UserStates[userId];
            switch (userState.Step)
            {
                case 0:
                    userState.Step = 1;
                    await Ext.SendMsg(
                              bot: botClient,
                              chatId: chatId,
                              Msg: "Вы создаете список окошек на Полный месяц.\n"+
                                   "Пожалуйста, следуйте инструкциям!\n\n"+
                                   "Введите количество дней с окошками");
                    break;
                case 1:
                    userState.Step = 2;
                    if (Bot.daysCount == 0) { Bot.daysCount = Convert.ToInt32(msgText); }
                    await Ext.SendMsg(
                              bot: botClient,
                              chatId: chatId,
                              Msg: $"Введите День окошек в формате: \"01.01\"");
                    break;
                case 2:
                    userState.Step = 3;
                    Bot.dates.Add(msgText, new());
                    Bot.lastMsg = upd.Message;
                    await Ext.SendMsg(
                              bot: botClient,
                              chatId: chatId,
                              Msg: $"Введите Часы окошек на {msgText}, разделяя пробелом.\n" +
                                   "Пример: \"10:00 12:00 14:00\"");                    
                        break;
                case 3:
                    for(int i = 0; i < msgText.Split(" ").Length; i++)
                    {
                        Bot.dates[Bot.lastMsg.Text].Add(msgText.Split(" ")[i]);
                    }
                    if (Bot.dates.Count < Bot.daysCount)
                    {
                        userState.Step = 1;
                        await AddNewMonth(botClient, upd);
                    }
                    else
                    {
                        userState.Step = 4;
                        await AddNewMonth(botClient, upd);
                    }
                    break;
                case 4:
                    userState.Step = 5;
                    string result = "";
                    foreach(var day in Bot.dates)
                    {
                        result += $"{day.Key}:\n";
                        foreach(var time in day.Value)
                        {
                            result += $"{time};\t";
                        }
                        result += $"\n";
                    }
                    await Ext.SendMsg(
                              bot: botClient,
                              chatId: chatId,
                              Msg: "Все даты зафиксированы, проверьте полный список на месяц:\n\n"+
                                  $"{result}\n"+
                                   "Если нужно исправить окошки, то введите День окошек\n"+
                                   "Подлежащий изменению, в формате \"01.01\"\n" +
                                   "Если все указано верно, введите \"Да\"");
                    break;
                case 5:
                    if (msgText.ToLower() == "да")
                    {
                        Bot.data.AddColWindows(Bot.dates);
                        UserStateManager.UserStates.Remove(userId);
                    }
                    else
                    {
                        Bot.lastMsg = upd.Message;
                        await Ext.SendMsg(
                                  bot: botClient,
                                  chatId: chatId,
                                  Msg: $"Введите исправленное Время окошек на {msgText}\n"+
                                        "В формате \"10:00 12:00 14:00\":");
                        userState.Step = 6;
                    }
                    break;
                case 6:
                    userState.Step = 4;
                    Bot.dates[Bot.lastMsg.Text] = msgText.Split(" ").ToList();
                    await AddNewMonth(botClient, upd);
                    UserStateManager.UserStates.Remove(userId);
                    break;
            }
        }

        /// <summary>
        /// Метод удаления окошка занятого в обход бота
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task TakeWindow(ITelegramBotClient botClient, Update upd)
        {
            var userId = upd.Message.From.Id;
            var chatId = upd.Message.Chat.Id;
            var msgText = upd.Message.Text;
            var userState = UserStateManager.UserStates[userId];
            switch(userState.Step)
            {
                case 0:
                    await Ext.SendMsg(
                            bot: botClient,
                            chatId: chatId,
                            Msg: "Введите дату занятого окошка\n" +
                                 "В формате \"01.01\"");
                    userState.Step = 1;
                    break;
                case 1:
                    if (msgText != null)
                    {
                        Bot.result += msgText;
                        await Ext.SendMsg(
                                bot: botClient,
                                chatId: chatId,
                                Msg: $"Выбрана дата {Bot.result}\n"+
                                      "Введите время занятого окошка\n"+
                                      "В формате \"00:00\"");
                        userState.Step = 2;
                    }
                    else
                    {
                        await Ext.SendMsg(
                                bot: botClient,
                                chatId: chatId,
                                Msg: $"Ошибка ввода, попробуйте еще раз.");
                    }
                    break;
                case 2:
                    if (msgText != null)
                    {
                        Bot.result += " " + msgText;
                        await Ext.SendMsg(
                            bot: botClient,
                            chatId: chatId,
                            Msg: "Выбранное окошко:\n"+
                                 $"{Bot.result.Split(" ")[0]} в {Bot.result.Split(" ")[1]}\n"+
                                 "Если все верно, то введите \"Да\"\n"+
                                 "Если есть ошибки, введите любой текст, выбор начнется сначала");
                        userState.Step = 3;
                    }
                    else
                    {
                        await Ext.SendMsg(
                                bot: botClient,
                                chatId: chatId,
                                Msg: $"Ошибка ввода, попробуйте еще раз.");
                    }
                    break;
                case 3:
                    if (msgText.ToLower() == "да")
                    {
                        Bot.data.TakeWindow(Bot.result);
                        await Ext.SendMsg(
                                bot: botClient,
                                chatId: chatId,
                                Msg: "Окошко успешно удалено!");
                        UserStateManager.UserStates.Remove(userId);
                    }
                    else
                    {
                        Bot.result = "";
                        userState.Step = 0;
                        await TakeWindow(botClient, upd);
                    }
                    break;
            }
        }

        /// <summary>
        /// Метод автоудаления неактуальных записей клиентов
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="upd"></param>
        /// <returns></returns>
        public static async Task DeleteNotesAuto(ITelegramBotClient botClient, Update upd)
        {
            var clients = Bot.data.GetAllClients();
            if (clients.Count > 0)
            {
                foreach (var client in clients)
                {
                    if (client.Value.ClientNote.Date.Split(" ")[0] == DateTime.Now.Date.ToString("dd.MM"))
                    {
                        int res = 0;
                        Bot.data.DeleteNote(client.Key, out res);
                        Bot.data.TakeWindow(client.Value.ClientNote.Date);
                    }
                }
            }
        }

    }
}
