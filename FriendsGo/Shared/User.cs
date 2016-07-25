using System;
using System.Runtime.InteropServices;

namespace Shared
{
    public class User
    {
        public string UserName { get; set; }
        public Guid Id { get; private set; }
        public string FullName { get; set; }

        public User(string name)
        {
            Id = new Guid();
            UserName = name;
        }
    }
}