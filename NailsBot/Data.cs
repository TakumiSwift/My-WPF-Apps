using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailsBot
{
    public class Data
    {
        
        /// <summary>
        /// Коллекция путей до файлов
        /// </summary>
        private Dictionary<string,string> paths;

        /// <summary>
        /// XDocument файла с записями и клиентами
        /// </summary>
        private XDocument data;

        /// <summary>
        /// XDocument файла с окошками
        /// </summary>
        private XDocument windows;

        /// <summary>
        /// Коллекция всех клиентов по id
        /// </summary>
        private static Dictionary<string, Client> allClients;

        /// <summary>
        /// Коллекция id администраторов
        /// </summary>
        private List<long> admindID;

        public Data()
        {
            paths = new Dictionary<string, string>
            {
                { "data" , @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\data.xml" },
                { "reference" , @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\Reference\"},
                { "price1" , @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\price\price1.jpg" },
                { "price2" , @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\price\price2.jpg" },
                { "windows", @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\windows\windows.xml" },
                { "admins", @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\administrators\admins.xml" },
                { "video", @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\location\IMG_2032.MOV" },
                { "location", @"F:\Visual Studio\Projects\AppsWPF\NailsBot\data\location\location.jpg" }
            };
            data = XDocument.Load(paths["data"]);
            windows = XDocument.Load(paths["windows"]);
            LoadAllClients(out allClients);
            admindID = new();
        }

        /// <summary>
        /// Метод сохранения записи в XML
        /// </summary>
        /// <param name="dat">Коллекция данных пользователя</param>
        public void AddNote(List<string> dat)
        {
            data.Element("Notes").Add(CreateNote(dat));
            data.Save(paths["data"]);
        }

        /// <summary>
        /// Метод замены записи по id пользователя
        /// </summary>
        /// <param name="dat">Измененные данные пользователя</param>
        public void ReplaceNote(List<string> dat)
        {
            data.Descendants("Notes")
                .Descendants("Note")
                .ToList()
                .Find(x => x.Element("Client").Value == dat[0])
                .Remove();
            AddNote(dat);
        }

        /// <summary>
        /// Метод удаления записи по id пользователя
        /// </summary>
        /// <param name="id">id удалемого пользователя</param>
        public void DeleteNote(string id, out int res )
        {
            try
            {
                var client = data.Descendants("Notes")
                                 .Descendants("Note")
                                 .ToList()
                                 .Find(x => x.Element("Client").Value == id);
                ReturnWindow(client.Element("Date").Value);
                client.Remove();
                data.Save(paths["data"]);
                res = 1;
            }
            catch
            { res = 0; }
        }

        /// <summary>
        /// Метод создания записи для сохранения в XML
        /// </summary>
        /// <param name="lines">Коллекция данных клиента</param>
        /// <returns>XElement записи</returns>
        private XElement CreateNote(List<string> lines)
        {
            XElement client = new("Client",new XAttribute("name", lines[1]), lines[0]);
            XElement phone = new("Phone", lines[2]);
            XElement service = new("Service", lines[3]);
            XElement date = new("Date", lines[4]);
            XElement reference = new("Reference", lines[5]);
            XElement chatId = new("ChatId", lines[6]);
            XElement note = new("Note");
            note.Add(client,phone,service,date,reference,chatId);
            return note;
        }

        /// <summary>
        /// Метод получения путей до файлов бота
        /// </summary>
        /// <param name="name">Тег файла в коллекции</param>
        /// <returns>Путь к файлу</returns>
        public string GetPaths(string name)
        {
            return paths[name];
        }

        /// <summary>
        /// Получение окошек из XML
        /// </summary>
        /// <param name="month">Текущий месяц</param>
        /// <param name="year">Текущий год</param>
        /// <returns>Коллекция дат и времени</returns>
        public Dictionary<string, List<string>> GetWindows(string month, string year)
        {
            Dictionary<string, List<string>> result = new();
            var xml = windows.Descendants("Windows")
                             .Descendants("Col")
                             .Descendants("Day")
                             .ToList();
            foreach(var item in xml)
            {
                if(item.Parent.Attribute("year").Value == year)
                {
                    if(item.Parent.Attribute("month").Value == month)
                    {
                        var s = item.Elements("Time").ToList();
                        List<string> l = new();
                        foreach(var i in s)
                        {
                            l.Add(i.Value);
                        }
                        result.Add(item.Attribute("num").Value,
                                   l);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Метод загрузки списка клиентов из файла
        /// </summary>
        /// <param name="allClients">Коллекция всех клиентов по их id</param>
        private void LoadAllClients(out Dictionary<string,Client> allClients)
        {
            allClients = new();
            var xml = data.Descendants("Notes")
                          .Descendants("Note")
                          .ToList();
            foreach(var item in xml)
            {
                if (item.Element("ChatId").Value != "")
                {
                    allClients.Add(
                        item.Element("Client").Value,
                        new Client(
                            item.Element("Client").Attribute("name").Value,
                            item.Element("Client").Value,
                            item.Element("Phone").Value,
                            new Note(
                                item.Element("Service").Value,
                                item.Element("Date").Value,
                                item.Element("Reference").Value),
                            Convert.ToInt64(item.Element("ChatId").Value)));
                }
            }
        }

        /// <summary>
        /// Метод получения клиента по его id
        /// </summary>
        /// <param name="id">id клиента</param>
        /// <param name="client">экземпляр клиента</param>
        public void GetClientById(string id, out Dictionary<long,Client> client)
        {
            LoadAllClients(out allClients);
            client = Bot.users;
            if (client.ContainsKey(Convert.ToInt64(id))) { }
            else { client.Add(Convert.ToInt64(allClients[id].Id), allClients[id]); }
        }

        /// <summary>
        /// Метод проверки наличия id пользователя в базе
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>лог. ответ</returns>
        public bool ContainClient(string id)
        {
            LoadAllClients(out allClients);
            bool result = allClients.ContainsKey(id);
            return result;
        }

        /// <summary>
        /// Метод проверки пользователя на Админа
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>Лог. решение</returns>
        public bool IsAdmin(long id)
        {
            bool result = false;
            var doc = XDocument.Load(paths["admins"])
                               .Descendants("Admins")
                               .Descendants("Admin")
                               .ToList();
            int i = 0;
            foreach(var item in doc)
            {
                if (Convert.ToInt64(item.Value) == id) { result = true; }
                if( admindID.Count > 0 )
                {
                    admindID.Remove(Convert.ToInt64(item.Value));
                }
                admindID.Insert(i,Convert.ToInt64(item.Value));
                i++;
            }
            return result;
        }

        /// <summary>
        /// Метод получения коллекции всех клиентов
        /// </summary>
        /// <param name="clients"></param>
        public Dictionary<string, Client> GetAllClients()
        {
            LoadAllClients(out allClients);
            return allClients;
        }

        /// <summary>
        /// Проверка наличия фото референса в записи
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns></returns>
        public static string HavePhoto(long id)
        {
            try
            {
                using (var stream = System.IO.File.OpenRead(allClients[Convert.ToString(id)].ClientNote.Reference))
                { }
                return allClients[Convert.ToString(id)].ClientNote.Reference;
            }
            catch
            { }
            return "Без фото";
        }

        /// <summary>
        /// Метод удаления окошка при записи на него
        /// </summary>
        /// <param name="date">Дата и время окошка</param>
        public void TakeWindow(string date)
        {
            var day = windows.Root
                             .Elements("Col")
                             .ToList()
                             .Find(x => x.Attribute("year").Value == Convert.ToString(DateTime.Now.Year)
                                     && x.Attribute("month").Value == Convert.ToString(DateTime.Now.Month))
                             .Elements("Day")
                             .ToList()
                             .Find(x => x.Attribute("num").Value == date.Split(" ")[0].Split(".")[0]);
            if(day.Elements("Time").ToList().Count > 1)
            {
                day.Elements("Time")
                   .ToList()
                   .Find(x => x.Value == date.Split(" ")[1])
                   .Remove();
            }
            else
            {
                day.Remove();
            }
            windows.Save(paths["windows"]);
        }

        /// <summary>
        /// Метод проверки наличия введенного окошка в базе
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Лог. ответ</returns>
        public bool TryFindWindow(string date)
        {
            bool result = true;
            try
            {
                XElement x = windows.Root
                                    .Elements("Col")
                                    .ToList()
                                    .Find(x => x.Attribute("year").Value == Convert.ToString(DateTime.Now.Year)
                                            && x.Attribute("month").Value == Convert.ToString(DateTime.Now.Month))
                                    .Elements("Day")
                                    .ToList()
                                    .Find(x => x.Attribute("num").Value == date.Split(" ")[0].Split(".")[0])
                                    .Elements("Time")
                                    .ToList()
                                    .Find(x => x.Value == date.Split(" ")[1]);
                if (x == null) { result = false; }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Метод возврата окошка после отмены записи
        /// </summary>
        /// <param name="date"></param>
        public void ReturnWindow(string date)
        {
            var days = windows.Root
                             .Elements("Col")
                             .ToList()
                             .Find(x => x.Attribute("year").Value == Convert.ToString(DateTime.Now.Year)
                                     && x.Attribute("month").Value == Convert.ToString(DateTime.Now.Month))
                             .Elements("Day")
                             .ToList();
            try
            {
                days.Find(x => x.Attribute("num").Value == date.Split(" ")[0].Split(".")[0])
                    .Add(new XElement("Time",date.Split(" ")[1]));
                var times = days.Find(x => x.Attribute("num").Value == date.Split(" ")[0].Split(".")[0])
                    .Elements("Time")
                    .ToList();
                times.Sort((x, y) =>
                {
                    int xV = Convert.ToInt32(x.Value.Split(":")[0]);
                    int yV = Convert.ToInt32(y.Value.Split(":")[0]);
                    return xV.CompareTo(yV);
                });
                days.Find(x => x.Attribute("num").Value == date.Split(" ")[0].Split(".")[0])
                    .ReplaceNodes(times);
            }
            catch
            {
                days.Add(new XElement("Day", 
                                      new XAttribute("num",date.Split(" ")[0].Split(".")[0]),
                                      new XElement("Time",date.Split(" ")[1])));
            }
            days.Sort((x, y) =>
            {
                int xV = Convert.ToInt32(x.Attribute("num").Value);
                int yV = Convert.ToInt32(y.Attribute("num").Value);
                return xV.CompareTo(yV);
            });
            windows.Root
                   .Elements("Col")
                   .ToList()
                   .Find(x => x.Attribute("year").Value == Convert.ToString(DateTime.Now.Year)
                           && x.Attribute("month").Value == Convert.ToString(DateTime.Now.Month))
                   .ReplaceNodes(days);
            windows.Save(paths["windows"]);
        }

        /// <summary>
        /// Метод удаления окошек вплоть до текущей даты, включительно!
        /// </summary>
        /// <param name="dates"></param>
        public void DeleteWindows(string date)
        {
            List<XElement> lst = new();
            var days = windows.Root
                   .Elements("Col")
                   .ToList()
                   .Find(x => x.Attribute("year").Value == date.Split(".")[2]
                           && x.Attribute("month").Value == date.Split(".")[1])
                   .Elements("Day")
                   .ToList();
            foreach(var x in days)
            {
                if (Convert.ToInt32(x.Attribute("num").Value) > Convert.ToInt32(date.Split(".")[0]))
                {
                    lst.Add(x);
                }
            }
            windows.Root
                   .Elements("Col")
                   .ToList()
                   .Find(x => x.Attribute("year").Value == date.Split(".")[2]
                           && x.Attribute("month").Value == date.Split(".")[1])
                   .ReplaceNodes(lst);
            windows.Save(paths["windows"]);
        }

        /// <summary>
        /// Метод получения коллекции id администраторов
        /// </summary>
        /// <returns></returns>
        public List<long> GetAdminId()
        {
            return this.admindID;
        }

        /// <summary>
        /// Метод добавления новых окошек на месяц
        /// </summary>
        /// <param name="dates">коллекция дат с временем окошек на месяц</param>
        public void AddColWindows(Dictionary<string, List<string>> dates)
        {
            XElement day = new("Day", new XAttribute("num", ""));
            XElement time = new("Time");
            XElement col = new("Col", new List<XAttribute>() { new("month",""), new("year","")});
            day.Add(time);
            col.Add(day);
            int i = 0;
            foreach(var item in dates)
            {
                if (i == 0)
                {
                    col.Attribute("month").Value = item.Key.Split(".")[1];
                    col.Attribute("year").Value = Convert.ToString(DateTime.Now.AddMonths(1).Year);
                    col.Element("Day").Attribute("num").Value = item.Key.Split(".")[0];
                    for (int j = 0; j < item.Value.Count; j++)
                    {
                        if (j == 0)
                        {
                            col.Element("Day").Element("Time").Value = item.Value[j];
                        }
                        else
                        {
                            col.Element("Day").Add(new XElement("Time", item.Value[j]));
                        }
                    }
                    i++;
                }
                else
                {
                    XElement day_ = new("Day", new XAttribute("num", ""));
                    day_.Attribute("num").Value = item.Key.Split(".")[0];
                    foreach (var e in item.Value)
                    {
                        day_.Add(new XElement("Time", e));
                    }
                    col.Add(day_);
                }
            }
            i = 0;
            windows.Root.Add(col);
            windows.Save(paths["windows"]);
        }

    }
}
