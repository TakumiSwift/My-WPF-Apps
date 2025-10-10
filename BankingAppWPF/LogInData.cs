using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using NPOI.Util;

namespace BankingAppWPF
{
    internal class LogInData
    {

        /// <summary>
        /// Путь к файлу с сотрудниками
        /// </summary>
        private static string path;

        /// <summary>
        /// Коллекция Логин-Пароль сотрудников(пл)
        /// </summary>
        private Dictionary<string, SecureString> AuthorizationData;

        /// <summary>
        /// Коллекция с Персональными данными сотрудников(пл)
        /// </summary>
        private List<string> PersonalData;

        /// <summary>
        /// Делегат возврата строкового значения параметра сотрудника
        /// </summary>
        /// <typeparam name="T1">Тип департамента сотрудника</typeparam>
        /// <param name="WorkerDepartment">Департамент сотрудника</param>
        /// <returns>Параметр сотрудника в строковом типе</returns>
        public delegate string ReturnTypeString<T1>(T1 WorkerParametr);

        public LogInData(string Path)
        {
            path = Path; //@"WorkerData/WorkerData.xml"; //@"ClientData/ClientData.xml";
            AuthorizationData = new Dictionary<string, SecureString>();
            LoadLogInData(AuthorizationData, path);
        }


        /// <summary>
        /// Метод загрузки пар Логин-Пароль
        /// </summary>
        /// <param name="LoadedData">Изначальная коллекция Логин-Пароль</param>
        /// <returns>Заполненная из файла коллекция Логин-Пароль</returns>
        private Dictionary<string, SecureString> LoadLogInData(Dictionary<string, SecureString> LoadedData, string Path)
        {
            if (Path == @"WorkerData/WorkerData.xml")
            {
                var xml = XDocument.Load(path)
                                   .Descendants("Workers")
                                   .Descendants("Departments")
                                   .Descendants("Department")
                                   .Descendants("Worker")
                                   .ToList();
                foreach (var item in xml)
                {
                    LoadedData.Add(item.Element("Login").Value,
                                   PasswordEncrypt(item.Element("Password").Value));
                }
            }
            else
            {
                var xml = XDocument.Load(path)
                                   .Descendants("Clients")
                                   .Descendants("Departments")
                                   .Descendants("Department")
                                   .Descendants("Client")
                                   .ToList();
                foreach (var item in xml)
                {
                    LoadedData.Add(item.Element("LoginData").Element("Login").Value,
                                   PasswordEncrypt(item.Element("LoginData").Element("Password").Value));
                }
            }
            return LoadedData;
            
        }

        /// <summary>
        /// Метод проверки существования пользователя
        /// </summary>
        /// <param name="log">Логин проверяемого пользователя</param>
        /// <param name="pass">Пароль проверяемого пользователя</param>
        /// <returns>Логический ответ наличия пользователя</returns>
        public bool InputCheck(string log, string pass)
        {
            bool flag = true;
            if (AuthorizationData.ContainsKey(log))
            {
                if (ConvertToInsecureString(AuthorizationData[log]) == pass) { }
                else {  flag = false; }
            }
            else { flag = false; }
            return flag;
        }

        /// <summary>
        /// Метод зашифровки пароля пользователя
        /// </summary>
        /// <param name="password">Введенный пароль</param>
        /// <returns>Зашифрованный пароль</returns>
        public SecureString PasswordEncrypt(string password)
        {
            if (password == null) { return null; }
            SecureString pass = new SecureString();
            foreach(var e in password)
            {
                pass.AppendChar(e);
            }
            return pass;
        }

        /// <summary>
        /// Раскодирование пароля
        /// </summary>
        /// <param name="secureString">Закодированный пароль</param>
        /// <returns>Строка раскодированного пароля</returns>
        static string ConvertToInsecureString(SecureString secureString)
        {
            if (secureString == null)
                return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                // Декодируем SecureString в BSTR (строку в формате COM)
                unmanagedString = Marshal.SecureStringToBSTR(secureString);

                // Преобразуем BSTR в managed string
                return Marshal.PtrToStringBSTR(unmanagedString);
            }
            finally
            {
                // ОБЯЗАТЕЛЬНО очищаем неуправляемую память
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(unmanagedString);
                }
            }
        }

        /// <summary>
        /// Метод получения экземпляра Client
        /// </summary>
        /// <param name="item">XElement Client в БД</param>
        /// <returns>Заполненный экземпляр Client</returns>
        private static Client GetClientFromXML(XElement item)
        {
            string fullName = item.Element("PersonalData")
                                  .Element("Fullname")
                                  ?.Value;
            ulong phone = Convert.ToUInt64(item.Element("PersonalData")
                                               .Element("Phone")
                                               ?.Value);
            ulong passport = Convert.ToUInt64(item.Element("PersonalData")
                                                  .Element("Passport")
                                                  ?.Value);
            ulong salary = Convert.ToUInt64(item.Element("PersonalData")
                                                .Element("Salary")
                                                ?.Value);               //
            var accountNumbers = GetClientAccounts(item);
            string directionTypeName = item.Parent.Attribute("direction").Value;        //
            object directionInstance = TypeFromString(directionTypeName);
            Client User = new Client(fullName,
                                     phone,
                                     passport,
                                     salary,
                                     accountNumbers,
                                     directionInstance as Department);
            return User;
        }

        /// <summary>
        /// Метод получение типа Счета
        /// </summary>
        /// <param name="item">XElement Счета</param>
        /// <returns>Экземпляр типа Счета</returns>
        private static BankAccountType GetAccountType(XElement item)
        {
            BankAccountType b = new DebitAccount(DateTime.Now);
            object test = TypeFromString(item.Parent.Attribute("type").Value);
            switch (test as BankAccountType)
            {
                case DebitAccount:
                    BankAccountType dA = new DebitAccount(Convert.ToDateTime(item.Element("OpeningDate").Value));
                    b = dA;
                    break;
                case CreditAccount:
                    TimeSpan tS = new TimeSpan(Convert.ToInt32(item.Element("PaymentPeriod").Value) * 30, 0, 0, 0);
                    BankAccountType cA = new CreditAccount(Convert.ToByte(item.Element("InterestRate").Value),
                                                            Convert.ToDateTime(item.Element("OpeningDate").Value),
                                                            tS);
                    b = cA;
                    break;
                case SavingsAccount:
                    TimeSpan ts = new TimeSpan(Convert.ToInt32(item.Element("PaymentPeriod").Value)*30,0,0,0);
                    BankAccountType sA = new SavingsAccount(Convert.ToByte(item.Element("InterestRate").Value),
                                                            Convert.ToDateTime(item.Element("OpeningDate").Value),
                                                            ts);
                    b = sA;
                    break;
            }
            return b;
        }

        /// <summary>
        /// Метод получения Коллекции Счетов Клиента
        /// </summary>
        /// <param name="item">XElement Client в БД</param>
        /// <returns>Выгруженная Коллекция Счетов клиента</returns>
        private static List<BankAccount> GetClientAccounts(XElement item)
        {
            List<BankAccount> accountNumbers = new();
            var newXml = XDocument.Load(@"AccountData/AccountData.xml")
                                  .Descendants("Accounts")
                                  .Descendants("AccountType")
                                  .Descendants("Account")
                                  .ToList();
            foreach (var e in newXml)
            {
                if (item.Element("PersonalData")
                       .Element("AccountNumbers")
                       .Value
                       .Split(',')
                       .ToList()
                       .Contains(e.Element("Number").Value))
                {
                    BankAccountType b = GetAccountType(e);
                    accountNumbers.Add(new BankAccount(Convert.ToUInt32(e.Element("Number").Value),
                                                       Convert.ToUInt64(e.Element("MoneyAmount").Value),
                                                       b));
                }
            }
            return accountNumbers;
        }
        
        /// <summary>
        /// Метод выгрузки клиентов департамента
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public static List<Client> GetMyClients(Worker worker)
        {
            var doc = XDocument.Load(@"ClientData/ClientData.xml");
            var xml = doc.Descendants("Clients")
                         .Descendants("Departments")
                         .Descendants("Department")
                         .Descendants("Client")
                         .ToList();
            List<Client> result = new List<Client>();
            foreach(var item in xml)
            {
                if ((TypeFromString(item.Parent.Attribute("direction").Value) as Department).GetType() == worker.Department.GetType())
                {
                    result.Add(GetClientFromXML(item));
                }
            }
            return result;
        }

        /// <summary>
        /// Метод загрузки данных пользователя на основе его логина
        /// </summary>
        /// <param name="login">Логин существующего пользователя</param>
        /// <returns>Экземпля авторизирующегося пользователя</returns>
        public ILogining GetAuthorizatedUser (string login)
        {
            if (path == @"WorkerData/WorkerData.xml")
            {
                var xml = XDocument.Load(path)
                                   .Descendants("Workers")
                                   .Descendants("Departments")
                                   .Descendants("Department")
                                   .Descendants("Worker")
                                   .ToList();
                foreach (var item in xml)
                {
                    if (item.Element("Login").Value == login)
                    {
                        string fullName = item.Attribute("fullname")?.Value;
                        string positionTypeName = item.Attribute("position")?.Value;
                        string directionTypeName = item.Parent.Attribute("direction").Value;
                        // Создаем экземпляры через рефлексию
                        object positionInstance = TypeFromString(positionTypeName);
                        object directionInstance = TypeFromString(directionTypeName);
                        // Создаем экземпляр Worker
                        Worker User = new(fullName,
                                          positionInstance as Position,
                                          directionInstance as Department);
                        return User;
                    }
                }
            }
            else
            {
                var xml = XDocument.Load(path)
                                   .Descendants("Clients")
                                   .Descendants("Departments")
                                   .Descendants("Department")
                                   .Descendants("Client")
                                   .ToList();
                foreach (var item in xml)
                {
                    if (item.Element("LoginData").Element("Login").Value == login)
                    {                        
                        Client User = GetClientFromXML(item);
                        return User;
                    }
                }
            }
            return default;    
        }

        /// <summary>
        /// Метод создания экземпляра типа по его названию
        /// </summary>
        /// <param name="typeName">Имя типа</param>
        /// <returns>Экземпляр типа в object</returns>
        public static object TypeFromString (string typeName)
        {
            Type newType = Type.GetType($"BankingAppWPF.{typeName}");
            object typeFS = Activator.CreateInstance(newType, new object[] { typeName });
            return typeFS;
        }

        /// <summary>
        /// Метод получения последнего номера счета заданного типа в базе
        /// </summary>
        /// <param name="accountType">Тип искомого счета</param>
        /// <returns></returns>
        public static uint GetLastAccountNumber (BankAccountType accountType)
        {
            var xml = XDocument.Load(@"AccountData/AccountData.xml")
                               .Descendants("Accounts")
                               .Descendants("AccountType")
                               .Descendants("Account")
                               .ToList();
            List<uint> last = new();
            switch(accountType)
            {
                case DebitAccount:
                    foreach(var item in xml)
                    {
                        if (item.Parent.Attribute("type").Value == "DebitAccount")
                        {
                            last.Add(Convert.ToUInt32(item.Element("Number").Value));
                        }
                    }
                    return last[last.Count - 1];
                case CreditAccount:
                    foreach (var item in xml)
                    {
                        if (item.Parent.Attribute("type").Value == "CreditAccount")
                        {
                            last.Add(Convert.ToUInt32(item.Element("Number").Value));
                        }
                    }
                    return last[last.Count - 1];
                case SavingsAccount:
                    foreach (var item in xml)
                    {
                        if (item.Parent.Attribute("type").Value == "SavingsAccount")
                        {
                            last.Add(Convert.ToUInt32(item.Element("Number").Value));
                        }
                    }
                    return last[last.Count - 1];
            }
            return default;
        }

        /// <summary>
        /// Метод перевода средств
        /// </summary>
        /// <param name="client">Клиент, осуществляющий перевод</param>
        /// <param name="accountSend">Номер Счета-отправителя</param>
        /// <param name="accountReceive">Номер Счета-получателя</param>
        /// <param name="moneyAmount">Сумма перевода</param>
        /// <returns>Клиент с измененной суммой</returns>
        public static Client ChangeMoneyAmount (Client client, uint accountSend, uint accountReceive, ulong moneyAmount)
        {
            var doc = XDocument.Load(@"AccountData/AccountData.xml");
            var xml = doc.Descendants("Accounts")
                         .Descendants("AccountType")
                         .Descendants("Account")
                         .ToList();
            foreach(var item in xml)
            {
                if (item.Element("Number").Value == accountSend.ToString())
                {
                    item.Element("MoneyAmount").Value = $"{Convert.ToUInt32(item.Element("MoneyAmount").Value) - moneyAmount}";
                }
                else if (item.Element("Number").Value == accountReceive.ToString())
                {
                    item.Element("MoneyAmount").Value = $"{Convert.ToUInt32(item.Element("MoneyAmount").Value) + moneyAmount}";
                }
            }
            doc.Save(@"AccountData/AccountData.xml");
            client.AccountNumbers.Find(x => x.AccountNumber == accountSend).MoneyAmount -= moneyAmount;
            return client;
        }

        /// <summary>
        /// Метод записи нового счета в XML-файлы
        /// </summary>
        /// <param name="newAccount">Новый счет</param>
        /// <param name="client">Клиент, кому закрепляется счет</param>
        public static void NewAccountToXml(BankAccount newAccount, Client client)
        {
        }

    }

    interface ILogining
    { }
}
