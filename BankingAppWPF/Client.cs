using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    public class Client : ILogining
    {

        private string firstName;

        private string lastName;

        private string surName;

        private ulong phone;

        private ulong passport;

        private ulong salary;

        private List<BankAccount> accountNumbers;

        private Department department;

        public string FirstName { get => this.firstName; set => this.firstName = value; }

        public string LastName { get => this.lastName; set => this.lastName = value; }

        public string SurName { get => this.surName; set => this.surName = value; }

        public ulong Phone { get => this.phone; set => this.phone = value; }

        public ulong Passport { get => this.passport; set => this.passport = value; }

        public ulong Salary { get => this.salary; set => this.salary = value; }

        public List<BankAccount> AccountNumbers { get => this.accountNumbers; set => this.accountNumbers = value; }

        public Department _Department { get => this.department; set => this.department = value; }

        /// <summary>
        /// Конструктор Client
        /// </summary>
        /// <param name="FullName">ФИО клиента</param>
        /// <param name="Phone">Номер телефона клиента</param>
        /// <param name="Passport">Паспорт клиента</param>
        /// <param name="Salary">Зарплата клиента</param>
        /// <param name="AccountNumbers">Коллекция бакн. счетов клиентов</param>
        /// <param name="department">Отдел обслуживания клиента</param>
        public Client (string FullName, ulong Phone, ulong Passport, ulong Salary, List<BankAccount> AccountNumbers, Department department)
        {
            this.FirstName = FullName.Split(' ')[0];
            this.LastName = FullName.Split(' ')[1];
            this.SurName = FullName.Split(' ')[2];
            this.Phone = Phone;
            this.Passport = Passport;
            this.Salary = Salary;
            this.AccountNumbers = AccountNumbers;
            this._Department = department;
        }

        /// <summary>
        /// Метод получения строкового значения типа счета
        /// </summary>
        /// <param name="index"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public string GetAccountString (int index, Client client)
        {
            return client.AccountNumbers[index].AccountType.TypeName;
        }

        /// <summary>
        /// Метод создания нового счета.
        /// Если нужен Дебетовый - ставить нули в ненужном.
        /// </summary>
        /// <param name="client">Клиент, кому добавляется счет</param>
        /// <param name="accountType">Тип нового счета</param>
        /// <param name="lastAccountNumber">Последний номер счета данного типа в базе</param>
        /// <param name="creditSize">Сумма кредита/вклада</param>
        /// <param name="creditProcent">Процентная ставка кредита/вклада</param>
        /// <param name="creditPeriod">Срок кредита/вклада</param>
        /// <returns></returns>
        public Client AddNewAccount (Client client,
                                     BankAccountType accountType,
                                     uint lastAccountNumber,
                                     ulong creditSize,
                                     byte creditProcent,
                                     TimeSpan creditPeriod)
        {
            switch(accountType)
            {
                case DebitAccount:
                    client.AccountNumbers.Add(new BankAccount(lastAccountNumber+1, 0, new DebitAccount(DateTime.Now)));
                    break;
                case CreditAccount:
                    client.AccountNumbers.Add(new BankAccount(lastAccountNumber+1,
                                                              creditSize,
                                                              new CreditAccount(creditProcent,
                                                                                DateTime.Now,
                                                                                creditPeriod)));
                    break;
                case SavingsAccount:
                    client.AccountNumbers.Add(new BankAccount(lastAccountNumber+1,
                                                              creditSize,
                                                              new SavingsAccount(creditProcent,
                                                                                DateTime.Now,
                                                                                creditPeriod)));
                    break;
            }
            return client;
        }

    }
}
