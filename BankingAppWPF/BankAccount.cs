using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    public class BankAccount
    {
       
        /// <summary>
        /// Номер банковского счета(пл)
        /// </summary>
        private uint accountNumber;

        /// <summary>
        /// Количество денег на счете(пл)
        /// </summary>
        private ulong moneyAmount;

        /// <summary>
        /// Тип банковского счета
        /// </summary>
        private BankAccountType accountType;

        /// <summary>
        /// Номер банковского счета(св)
        /// </summary>
        public uint AccountNumber { get => this.accountNumber; set => this.accountNumber = value; }

        /// <summary>
        /// Количество денег на счете(св)
        /// </summary>
        public ulong MoneyAmount { get => this.moneyAmount; set => this.moneyAmount = value; }

        /// <summary>
        /// Тип банковского счета(св)
        /// </summary>
        public BankAccountType AccountType { get => this.accountType; set => this.accountType = value; }

        /// <summary>
        /// Конструктор банковского счета
        /// </summary>
        /// <param name="accountNumber">Номер счета</param>
        /// <param name="moneyAmount">Количество денег на счете</param>
        /// <param name="accountType">Тип счета</param>
        public BankAccount (uint accountNumber, ulong moneyAmount, BankAccountType accountType)
        {
            this.accountNumber = accountNumber;
            this.moneyAmount = moneyAmount;
            this.accountType = accountType;
        }

    }
}
