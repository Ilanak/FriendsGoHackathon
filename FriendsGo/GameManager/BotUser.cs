using System;
using System.Runtime.InteropServices;
using GameManager;

namespace Shared
{
    public class BotUser : DocDbEntityBase
    {
        public string UserName { get; set; }

        public BotUser(string id,string userName = "")
        {
            TelegramId = id;
            UserName = userName;
        }
    }
}