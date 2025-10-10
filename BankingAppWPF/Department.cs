
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    public class Department
    {

        /// <summary>
        /// Строковое значение Департамента сотрудника(пл)
        /// </summary>
        protected string _department;

        /// <summary>
        /// Строковое значение Департамента сотрудника(св)
        /// </summary>
        protected string _Department { get => this._department; set => this._department = value; }

        protected Department(string Department)
        {
            this._Department = Department;
        }

        /// <summary>
        /// Метод возврата строкового значения Департамента сотрудника
        /// </summary>
        /// <param name="dep">Параметр департамента сотрудника</param>
        /// <returns>Строковое значение департамента сотрудника</returns>
        public string DepToString(Department dep)          
        {
            return $"{dep._Department}";
        }

    }
    public class Casual : Department
    {
        public Casual(string Department) : base(Department) { }        

        public string DepToString(Casual dep) { return base.DepToString(dep); }

    }

    public class Vip : Department
    {
        public Vip(string Department) : base(Department) { }

        public string DepToString(Vip dep) { return base.DepToString(dep); }

    }

    public class LegalEntity : Department
    {
        public LegalEntity(string Department) : base(Department) { }

        public string DepToString(LegalEntity dep) { return base.DepToString(dep); }

    }
}
