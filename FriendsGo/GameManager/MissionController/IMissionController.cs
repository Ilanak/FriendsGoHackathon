using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GoogleApi.Entities.Common;

namespace GameManager
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
            //this describes how many different submissions a mission will hold depending on the current level
            Levels = new Dictionary<int, int>();
            Levels[0] = 1;
            Levels[1] = 1;
            Levels[2] = 1;
            Levels[3] = 2;
            Levels[4] = 2;
            Levels[5] = 2;
            Levels[6] = 2;
            Levels[7] = 2;
            Levels[8] = 3;
        }

        public Mission GetMission(int level, Location startLocation, List<Location> previousLocations)
        {
            var mission = new Mission();

            for (int i = 0; i < Levels[level]; i++)
            {
                var submission = SubMissionsFactory.Create(SubMissionType.ExactLocation, level, startLocation);

                mission.SubMissions.Add(submission);
            }

            return mission;
        }
    }
}
