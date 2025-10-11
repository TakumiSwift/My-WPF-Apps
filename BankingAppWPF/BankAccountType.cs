using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    /// <summary>
    /// Тип банковского счета
    /// </summary>
    public class BankAccountType
    {

        #region Поля

        /// <summary>
        /// Тип счета в string(пл)
        /// </summary>
        protected string typeName;

        /// <summary>
        /// Тип счета(пл)
        /// </summary>
        protected Type accountType;

        /// <summary>
        /// Дата открытия счета(пл)
        /// </summary>
        protected DateTime accountOpeningDate;

        /// <summary>
        /// Процентная ставка счета(пл)
        /// </summary>
        protected byte interestRate;

        /// <summary>
        /// Срок кредита/вклада(пл)
        /// </summary>
        protected TimeSpan paymentPeriod;

        #endregion

        #region Свойства

        /// <summary>
        /// Тип счета в string(св)
        /// </summary>
        public string TypeName { get => this.typeName; set => this.typeName = value; }

        /// <summary>
        /// Тип счета(св)
        /// </summary>
        protected Type AccountType { get => this.accountType; set => this.accountType = value; }

        /// <summary>
        /// Дата открытия счета(св)
        /// </summary>
        protected DateTime AccountOpeningDate { get => this.accountOpeningDate; set => this.accountOpeningDate = value; }

        /// <summary>
        /// Процентная ставка счета(св)
        /// </summary>
        public byte InterestRate { get => this.interestRate; set => this.interestRate = value; }

        /// <summary>
        /// Срок кредита/вклада(св)
        /// </summary>
        public TimeSpan PaymentPeriod { get => this.paymentPeriod; set => this.paymentPeriod = value; }

        #endregion

        /// <summary>
        /// Конструктор Типа банковского счета
        /// </summary>
        /// <param name="accountType">Тип счета</param>
        /// <param name="accountOpeningDate">Дата открытия счета</param>
        protected BankAccountType (Type accountType, DateTime accountOpeningDate)
        { 
            this.AccountType = accountType;
            this.AccountOpeningDate = accountOpeningDate;
        }
        
        public string GetOpeningDate(BankAccountType bat)
        {
            return $"{Convert.ToString(bat.AccountOpeningDate.Date)}";
        }

        public string GetPaymentPeriod(BankAccountType bat)
        {
            return $"{(bat.PaymentPeriod/30).ToString(@"dd")}";
        }

        public string GetInterestRate(BankAccountType bat)
        {
            return Convert.ToString(bat.InterestRate);
        }

    }

    /// <summary>
    /// Дебетовый банковский счет
    /// </summary>
    public class DebitAccount : BankAccountType
    {
        /// <summary>
        /// Конструктор Дебетового банковского счета
        /// </summary>
        /// <param name="accountOpeningDate">Дата открытия счета</param>
        public DebitAccount (DateTime accountOpeningDate) : base(typeof(DebitAccount),accountOpeningDate)
        { base.TypeName = "Дебетовый счет"; }

        public DebitAccount (string s) : base(typeof(DebitAccount), DateTime.Now) { base.TypeName = "Дебетовый счет"; }

    }

    /// <summary>
    /// Кредитный банковский счет
    /// </summary>
    public class CreditAccount : BankAccountType
    {
        /// <summary>
        /// Конструктор Кредитного банковского счета
        /// </summary>
        /// <param name="interestRate">Процентная ставка по кредиту</param>
        /// <param name="accountOpeningDate">Дата открытия счета</param>
        /// <param name="paymentPeriod">Срок выплаты кредита</param>
        public CreditAccount (byte interestRate, DateTime accountOpeningDate, TimeSpan paymentPeriod)
            : base(typeof(CreditAccount),accountOpeningDate)
        {
            base.InterestRate = interestRate;
            base.PaymentPeriod = paymentPeriod;
            base.TypeName = "Кредитный счет";
        }

        public CreditAccount(string s) : base(typeof(CreditAccount), DateTime.Now) { base.TypeName = "Кредитный счет"; }

    }

    /// <summary>
    /// Вкладовый банковский счет
    /// </summary>
    public class SavingsAccount : BankAccountType
    {
        /// <summary>
        /// Конструктор Вкладового банковского счета
        /// </summary>
        /// <param name="interestRate">Процентная ставка по вкладу</param>
        /// <param name="accountOpeningDate">Дата открытия счета</param>
        /// <param name="paymentPeriod">Срок действия вклада</param>
        public SavingsAccount (byte interestRate, DateTime accountOpeningDate, TimeSpan paymentPeriod)
            :base(typeof(SavingsAccount), accountOpeningDate)
        {
            base.InterestRate = interestRate;
            base.PaymentPeriod = paymentPeriod;
            base.TypeName = "Сберегательный счет";
        }

        public SavingsAccount(string s) : base(typeof(SavingsAccount), DateTime.Now) { base.TypeName = "Сберегательный счет"; }

    }
}
