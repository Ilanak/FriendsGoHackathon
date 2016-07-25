using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi.Entities.Common;

namespace MissionController
{
    public class Mission
    {
        public Mission()
        {
            SubMissions = new List<SubMission>();
        }

        public List<SubMission> SubMissions;
    }

    public static class SubMissionsFactory
    {
        public static SubMission Create(SubMissionType type, int level, Location startLocation)
        {
            switch (type)
            {
                    case SubMissionType.ExactLocation:
                        return new ExactLocationSubMission(level);
                    default:
                        throw new ArgumentException();
            }
        }
    }

    public abstract class SubMission
    {        
    }

    public class ExactLocationSubMission : SubMission
    {
        public int NumberOfPlayers;

        public Location  ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission(int level)
        {
            NumberOfPlayers = level;
            
            // ExactLocation = 
            Duration = TimeSpan.MaxValue;
        }
    }

    public enum SubMissionType
    {
        Default = 0,
        ExactLocation = 1,
    }
    
}
