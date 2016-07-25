using System;

namespace Shared
{
    public class Group
    {
        public Guid Id { get; private set; }
        public int Level { get; set; }
        public DateTime CreateDate { get; private set; }

        public Group()
        {
            Id = new Guid();
            CreateDate = DateTime.UtcNow;
        }
    }
}