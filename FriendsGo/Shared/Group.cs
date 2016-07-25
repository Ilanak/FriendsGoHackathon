using System;
using System.Data.Common;
using System.Security.Policy;
using System.Xml.Schema;
using GoogleApi.Entities.Common;

namespace Shared
{
    public class Group
    {
        public Guid Id { get; private set; }
        public int Level { get; set; }
        public DateTime CreateDate { get; private set; }
        public Location StartLocation { get; private set; }
        public int Score { get; set; }

        public Group(Location location)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
            StartLocation = location;
            Score = 0;
        }
    }
}