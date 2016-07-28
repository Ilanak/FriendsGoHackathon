using System;
using System.Collections.Generic;
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
        public Dictionary<int, Mission> GeneratedMissions { get; set; }
        public string GroupName { get; set; }

        public Group(string id, string groupName, Location location)
        {
            TelegramId = id;
            GroupName = groupName;
            CreateDate = DateTime.UtcNow;
            StartLocation = location;
            Score = 0;
            Level = 0;
            GeneratedMissions = new Dictionary<int, Mission>();
        }
        
        public Mission GetCurrentMission()
        {
            if (GeneratedMissions.ContainsKey(Level))
            {
                return GeneratedMissions[Level];
            }
            else
            {
                return null;
            }
        }
    }
}