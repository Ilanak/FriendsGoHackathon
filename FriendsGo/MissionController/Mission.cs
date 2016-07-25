using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionController
{
    public class Mission
    {
        public List<SubMission> SubMissions;
    }

    public abstract class SubMission
    {
    }

    public class ExactLocationSubMission : SubMission
    {
        public int NumberOfPlayers;

        public Location  ExactLocation;

        public TimeSpan Duration;
    }


    
}
