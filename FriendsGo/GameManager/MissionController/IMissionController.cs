using System;
using System.Collections.Generic;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocode.Request;

namespace GameManager
{
    public interface IMissionController
    {
        Mission GetMission(int level, Location startLocation, List<Location> previousLocations);
    }

    public class MissionSettings
    {
        public MissionSettings(int subCount, int checkCount, int radius, int duration, bool randCity)
        {
            SubMissionCount = subCount;
            CheckinCount = checkCount;
            Radius = radius;
            CheckInCycleDuration = duration;
            RandomCity = randCity;
        } 
        public int SubMissionCount { set; get; }
        public int CheckinCount { set; get; }
        public int Radius { set; get; }
        public int CheckInCycleDuration { set; get; }
        public bool RandomCity { set; get; }

    }

    public class MissionController : IMissionController
    {
        public Dictionary<int, MissionSettings> LevelsSettings;
        private Random rand = new Random();

        public MissionController()
        {
            
            //this describes the levels difficulty
            //int - how many different submissions a mission will hold
            //int - how many checkin are required for *each* submission
            //int - a meter based radius for the submission to select location within
            //int - sec duration for checkins to happen 
            //bool - flag add city - randomize a new city to add to game
            LevelsSettings = new Dictionary<int, MissionSettings>();

            LevelsSettings[0] =     new MissionSettings(1, 1, 10,     600, false);
            LevelsSettings[1] =     new MissionSettings(1, 2, 10,     600, false);
            LevelsSettings[2] =     new MissionSettings(1, 2, 50,     600, false);
            LevelsSettings[3] =     new MissionSettings(1, 3, 10,     600, false);
            LevelsSettings[4] =     new MissionSettings(1, 3, 100,    600, false);
            LevelsSettings[5] =     new MissionSettings(2, 1, 10,     600, false);
            LevelsSettings[6] =     new MissionSettings(2, 1, 200,    600, false);
            LevelsSettings[7] =     new MissionSettings(2, 2, 100,    600, false);
            LevelsSettings[8] =     new MissionSettings(2, 2, 200,    600, false);
            LevelsSettings[9] =     new MissionSettings(3, 1, 10,     600, true);
            LevelsSettings[10] =    new MissionSettings(3, 2, 100,     600, false);
            LevelsSettings[11] =    new MissionSettings(3, 2, 2000,    600, false);
            LevelsSettings[12] =    new MissionSettings(3, 2, 1000,    600, true);
            LevelsSettings[13] =    new MissionSettings(3, 2, 10,      600, false);
            LevelsSettings[14] =    new MissionSettings(3, 2, 2000,      600, false);
            LevelsSettings[15] =    new MissionSettings(4, 2, 100,      600, true);
            LevelsSettings[16] =    new MissionSettings(4, 2, 1000,      600, false);
            LevelsSettings[17] =    new MissionSettings(4, 3, 100,      600, false);

        }

        public Mission GetMission(int level, Location startLocation, List<Location> previousLocations)
        {
            var mission = new Mission();
            MissionSettings settings = LevelsSettings[level];

            int subMissionCount = settings.SubMissionCount;
            if (settings.RandomCity)
            {
                //need to add a new submission of city type
                var submission = SubMissionsFactory.Create(SubMissionType.CityLocation, level, startLocation, settings.CheckinCount, settings.Radius, settings.CheckInCycleDuration);

                mission.SubMissions.Add(submission);
                subMissionCount --;
            }

            for (int i = 0; i < subMissionCount; i++)
            {

                int baseLocation = rand.Next(0, previousLocations.Count - 1);
            
                var submission = SubMissionsFactory.Create(SubMissionType.ExactLocation, level, previousLocations[baseLocation], settings.CheckinCount, settings.Radius, settings.CheckInCycleDuration);

                mission.SubMissions.Add(submission);
            }

            return mission;
        }

        public static string GetCityByCoordinates(double latitude, double longitude)
        {
            var placesRequest = new GeocodingRequest()
            {
                Key = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8",
                Sensor = true,
                Language = "en",
                Location = new Location(latitude, longitude),
            };

            var result = GoogleMaps.Geocode.Query(placesRequest);
            string city = "";

            foreach (var r in from r in result.Results from t in r.Types where t.ToString().Equals("LOCALITY") select r)
            {
                city = r.AddressComponents.First().LongName;
            }

            return city;
        }
    }
}
