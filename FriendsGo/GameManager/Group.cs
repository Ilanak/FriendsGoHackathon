using System;
using GoogleApi.Entities.Common;

namespace Shared
{
    public class Group
    {
        public string Id { get; private set; }
        public int Level { get; set; }
        public DateTime CreateDate { get; private set; }
        public Location StartLocation { get; private set; }
        public int Score { get; set; }

        public Group(string id, Location location)
        {
            Id = id;
            CreateDate = DateTime.UtcNow;
            StartLocation = location;
            Score = 0;
        }
    }
}