using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    public class Worker : ILogining
    {

        /// <summary>
        /// ФИО сотрудника(пл)
        /// </summary>
        private string fullname;

        /// <summary>
        /// Должность сотрудника(пл)
        /// </summary>
        private Position position;

        /// <summary>
        /// Отдел сотрудника(пл)
        /// </summary>
        private Department department;

        /// <summary>
        /// ФИО сотрудника(св)
        /// </summary>
        public string Fullname { get => this.fullname; set => this.fullname = value; }

        /// <summary>
        /// Должность сотрудника(св)
        /// </summary>
        public Position Position { get => this.position; set => this.position = value; }

        /// <summary>
        /// Отдел сотрудника(св)
        /// </summary>
        public Department Department { get => this.department; set => this.department = value; }

        /// <summary>
        /// Полный конструктор Worker
        /// </summary>
        /// <param name="fullname">ФИО сотрдуника</param>
        /// <param name="position">Должность сотрудника</param>
        /// <param name="department">Департамент сотрудника</param>
        public Worker(string fullname, Position position, Department department)
        {
            this.Fullname = fullname;
            this.Position = position;
            this.Department = department;
        }

        /// <summary>
        /// Автоконструктор Worker
        /// </summary>
        public Worker()
        {
            this.Fullname = "Фамилия%Имя%Отчество";
            this.Position = new Consultant("Должность");
            this.Department = new Casual("Департамент");
        }

    }
}
