using System;
using System.Runtime.InteropServices;

namespace Shared
{
    public class BotUser
    {
        public string Id { get; private set; }
        public string UserName { get; set; }

        public BotUser(string id)
        {
            Id = id;
        }
    }
}