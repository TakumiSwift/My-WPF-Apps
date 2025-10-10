using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace BankingAppWPF
{
    public class Offer
    {        

        static string path;

        static XDocument doc;

        bool flag;

        public Offer()
        {
            path = @"Offers/Offers.xml";
            doc = XDocument.Load(path);
            flag = true;
            ClientWindow.OfferShow += OffersCheck;
        }

        /// <summary>
        /// Метод добавления нового предложения для клиента
        /// </summary>
        /// <param name="account">Тип предложения</param>
        /// <param name="recipient">Клиент-получатель предложения</param>
        public void AddNewOffer(BankAccountType account, Client recipient)
        {
            var offer = new XElement("Offer");
            var offerData = new XElement("Data");
            var offerRecipient = new XElement("Recipient");
            var offerStatus = new XAttribute("status", "wait");
            var offerType = new XAttribute("type",$"{account.GetType().Name}");
            var offerId = new XAttribute("id", GetLastId());
            var offerProcent = new XAttribute("procent", $"{account.GetInterestRate(account)}");
            var offerPeriod = new XAttribute("period", $"{account.GetPaymentPeriod(account)}");
            var offerDate = new XAttribute("offerDate", $"{account.GetOpeningDate(account)}");
            var recipientPassport = new XAttribute("passport", $"{recipient.Passport}");
            offerData.Add(offerType, offerProcent, offerPeriod, offerDate);
            offerRecipient.Add(recipientPassport);
            offer.Add(offerStatus,offerId,offerData,offerRecipient);
            doc.Root.Add(offer);
            doc.Save(path);
        }

        /// <summary>
        /// Метод проверки наличия Предложений у Клиента
        /// </summary>
        /// <param name="user">Проверяемый Клиент</param>
        /// <param name="flag">Условие прогруженности окна Клиента</param>
        /// <returns>Коллекция предложений для Клиента</returns>
        private (List<BankAccountType> result, List<string> id) OffersCheck(Client user)
        {
            List<BankAccountType> result = new();
            List<string> id = new();
            var xml = doc.Descendants("Offers")
                         .Descendants("Offer")
                         .ToList();
            foreach(var item in xml)
            {
                if(Convert.ToUInt64(item.Element("Recipient").Attribute("passport").Value) == user.Passport)
                {
                    if (item.Attribute("status").Value == "wait")
                    {
                        if (flag)
                        {
                            MessageBox.Show("У вас есть новые предложения от Банка!");
                            flag = false;
                        }
                        BankAccountType bat = GetAccountType(item);
                        result.Add(bat);
                        id.Add(item.Attribute("id").Value);
                    }
                }
            }
                return (result, id);
        }

        /// <summary>
        /// Метод получение типа Счета
        /// </summary>
        /// <param name="item">XElement Предложения</param>
        /// <returns>Тип Счета</returns>
        private BankAccountType GetAccountType(XElement item)
        {
            object type = LogInData.TypeFromString(item.Element("Data").Attribute("type").Value);
            TimeSpan span = new TimeSpan(Convert.ToInt32(item.Element("Data")
                                                             .Attribute("period")
                                                             .Value), 0, 0, 0);
            switch (type as BankAccountType)
            {
                case CreditAccount:
                    return new CreditAccount(Convert.ToByte(item.Element("Data")
                                                                .Attribute("procent")
                                                                .Value),
                                             DateTime.Now.Date,
                                             span);
                case SavingsAccount:
                    return new SavingsAccount(Convert.ToByte(item.Element("Data")
                                                                 .Attribute("procent")
                                                                 .Value),
                                             DateTime.Now.Date,
                                             span);
            }
            return default;
        }

        /// <summary>
        /// Метод изменения статуса Предложения
        /// </summary>
        /// <param name="status">Новый статус string</param>
        /// <param name="passport">Паспорт клиента</param>
        public static void ChangeOfferStatus(string status, string id)
        {
            var xml = doc.Descendants("Offers")
                         .Descendants("Offer")
                         .ToList();
            XElement item = xml.Find(x => x.Attribute("id").Value == id);
            item.Attribute("status").Value = status;
            item.Attribute("id").Value = GetLastId();
            doc.Save(path);
        }

        /// <summary>
        /// Метод получения id последнего предложения 
        /// </summary>
        /// <returns>id последнего предложения</returns>
        private static string GetLastId()
        {
            var xml = doc.Descendants("Offers")
                         .Descendants("Offer")
                         .ToList();
            string id = "";
            List<int> numbers = new();
            foreach(var item in xml)
            {
                numbers.Add(Convert.ToInt32(item.Attribute("id").Value));
            }
            id = $"{numbers.Max()+1}";
            return id;
        }

    }
}
