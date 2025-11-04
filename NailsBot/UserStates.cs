using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailsBot
{
    // Хранилище состояний пользователей
    public static class UserStateManager
    {
        public static Dictionary<long, UserState> UserStates = new();
    }

    public class UserState
    {
        public string CurrentCommand { get; set; }

        public string ClientName { get; set; }

        public string Phone { get; set; }

        public string Service { get; set; }

        public string Date { get; set; }

        public string Reference { get; set; }

        public int Step { get; set; } = 0; // Текущий шаг диалога

    }
}
