using System;
using System.Runtime.InteropServices;

namespace Shared
{
    public class BotUser
    {
        public string UserName { get; set; }
        public Guid Id { get; private set; }
        public string FullName { get; set; }

        public BotUser(string name)
        {
            Id = Guid.NewGuid();
            UserName = name;
        }
    }
}