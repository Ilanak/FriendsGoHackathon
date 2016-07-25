using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MissionController
{
    public interface IMissionController
    {
        Mission GetMission(int level, Location startLocation, List<Location> previousLocations);
    }

    public class MissionController : IMissionController
    {
        public Dictionary<int, int> Levels;

        public MissionController()
        {
            Levels = new Dictionary<int, int>();
            Levels[0] = 1;
            Levels[1] = 2;
            Levels[2] = 3;
        }

        public Mission GetMission(int level, Location startLocation, List<Location> previousLocations)
        {
            var mission = new Mission();

            var submission = SubMissionsFactory.Create(SubMissionType.ExactLocation, level);
            
            mission.SubMissions.Add(submission);

            return mission;
        }
    }
}
