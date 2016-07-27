using System;
using GameManager;
using GoogleApi.Entities.Common;

namespace Shared
{
    public class Group : DocDbEntityBase
    {
        public int Level { get; set; }
        public DateTime CreateDate { get; private set; }
        public Location StartLocation { get; set; }
        public int Score { get; set; }

        public Group(string id, Location location)
        {
            TelegramId = id;
            CreateDate = DateTime.UtcNow;
            StartLocation = location;
            Score = 0;
        }
    }
}