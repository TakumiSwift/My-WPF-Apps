using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingAppWPF
{
    public class Position
    {

        /// <summary>
        /// Строковое значение должности сотрудника(пл)
        /// </summary>
        protected string _position;

        /// <summary>
        /// Строковое значение должности сотрудника(св)
        /// </summary>
        protected string _Position { get =>  _position; set => _position = value; }

        protected Position (string Position)
        {
            _Position = Position;
        }

        /// <summary>
        /// Метод возвращения строкового значения должности сотрудника
        /// </summary>
        /// <param name="pos">Параметр должности сотрудника</param>
        /// <returns>Строковое значение должности сотрудника</returns>
        public string PosToString(Position pos)
        {
            return $"{pos._Position}";
        }

    }
    public class HeadOfDepartment : Position
    {
        public HeadOfDepartment(string Position) : base(Position) { }

        /// <summary>
        /// Метод возвращения строкового значения должности сотрудника
        /// </summary>
        /// <param name="pos">Параметр должности сотрудника</param>
        /// <returns>Строковое значение должности сотрудника</returns>
        public string PosToString(HeadOfDepartment pos) { return base.PosToString(pos); }

    }

    public class Manager : Position
    {
        public Manager(string Position) : base(Position) { }

        /// <summary>
        /// Метод возвращения строкового значения должности сотрудника
        /// </summary>
        /// <param name="pos">Параметр должности сотрудника</param>
        /// <returns>Строковое значение должности сотрудника</returns>
        public string PosToString(Manager pos) { return base.PosToString(pos); }

    }

    public class Consultant : Position
    {
        public Consultant(string Position) : base(Position) { }

        /// <summary>
        /// Метод возвращения строкового значения должности сотрудника
        /// </summary>
        /// <param name="pos">Параметр должности сотрудника</param>
        /// <returns>Строковое значение должности сотрудника</returns>
        public string PosToString(Consultant pos) { return base.PosToString(pos); }

    }


}
