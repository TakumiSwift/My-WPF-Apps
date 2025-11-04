using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NailsBot
{
    public class Client
    {
        
        /// <summary>
        /// Имя клиента
        /// </summary>
        private string name;

        /// <summary>
        /// id тг-аккаунта клиента
        /// </summary>
        private string id;

        /// <summary>
        /// Номер телефона или @username клиента
        /// </summary>
        private string phone;

        /// <summary>
        /// Текущая запись клиента
        /// </summary>
        private Note clientNote;

        /// <summary>
        /// id чата с клиентом
        /// </summary>
        private long chatId;

        /// <summary>
        /// Имя клиента
        /// </summary>
        public string Name { get => this.name; set => this.name = value; }

        /// <summary>
        /// id тг-аккаунта клиента
        /// </summary>
        public string Id { get => this.id; set => this.id = value; }

        /// <summary>
        /// Номер телефона или @username клиента
        /// </summary>
        public string Phone { get => this.phone; set => this.phone = value; }

        /// <summary>
        /// Текущая запись клиента
        /// </summary>
        public Note ClientNote { get => this.clientNote; set => this.clientNote = value; }

        /// <summary>
        /// id чата с клиентом
        /// </summary>
        public long ChatId { get => this.chatId; set => this.chatId = value; }

        /// <summary>
        /// Конструктор клиента
        /// </summary>
        /// <param name="name">имя при записи</param>
        /// <param name="id">id тг-аккаунта</param>
        /// <param name="phone">телефон или @username</param>
        /// <param name="clientNote">экземпляр записи клиента</param>
        /// <param name="chatId">id чата клиента с ботом</param>
        public Client(string name, string id, string phone, Note clientNote, long chatId)
        {
            this.Name = name;
            this.Id = id;
            this.Phone = phone;
            this.ClientNote = clientNote;
            this.ChatId = chatId;
        }

        public Client() { }

    }
}
