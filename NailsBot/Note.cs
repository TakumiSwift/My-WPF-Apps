using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailsBot
{
    public class Note
    {

        /// <summary>
        /// Услуга записи
        /// </summary>
        private string service;

        /// <summary>
        /// Дата и Время записи
        /// </summary>
        private string date;

        /// <summary>
        /// Ссылка на референс
        /// </summary>
        private string reference;

        /// <summary>
        /// Услуга записи
        /// </summary>
        public string Service { get  => this.service; set => this.service = value; }

        /// <summary>
        /// Дата и Время записи
        /// </summary>
        public string Date { get => this.date; set => this.date = value; }

        /// <summary>
        /// Ссылка на референс
        /// </summary>
        public string Reference { get => this.reference; set => this.reference = value; }

        public Note(string service, string date, string reference)
        {
            this.Service = service;
            this.Date = date;
            this.Reference = reference;
        }

    }
}
