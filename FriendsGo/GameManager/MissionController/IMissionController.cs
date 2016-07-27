using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Maps.Geocode.Request;
using GoogleApi.Entities.Places.Details.Request;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;

namespace GameManager
{
    public interface IMissionController
    {
        Mission GetMission(int level, Location startLocation, List<Location> previousLocations);
    }

    public class MissionController : IMissionController
    {
        public Dictionary<int, Tuple<int, int, int, int, bool>> LevelsDifficulty;

        public MissionController()
        {
            //this describes the levels difficulty
            //int - how many different submissions a mission will hold
            //int - how many checkin are required for *each* submission
            //int - a meter based radius for the submission to select location within
            //int - sec duration for checkins to happen 
            //bool - flag add city - randomize a new city to add to game
            LevelsDifficulty = new Dictionary<int, Tuple<int, int, int, int, bool>>();
            LevelsDifficulty[0] = new Tuple<int, int, int, int, bool>(1, 1, 10,     600, false);
            LevelsDifficulty[1] = new Tuple<int, int, int, int, bool>(1, 2, 10,     600, false);
            LevelsDifficulty[2] = new Tuple<int, int, int, int, bool>(1, 2, 50,     600, false);
            LevelsDifficulty[3] = new Tuple<int, int, int, int, bool>(1, 3, 10,     600, false);
            LevelsDifficulty[4] = new Tuple<int, int, int, int, bool>(1, 3, 100,    600, false);
            LevelsDifficulty[5] = new Tuple<int, int, int, int, bool>(2, 1, 10,     600, false);
            LevelsDifficulty[6] = new Tuple<int, int, int, int, bool>(2, 1, 200,    600, false);
            LevelsDifficulty[7] = new Tuple<int, int, int, int, bool>(2, 2, 100,    600, false);
            LevelsDifficulty[8] = new Tuple<int, int, int, int, bool>(2, 2, 200,    600, false);
            LevelsDifficulty[9] = new Tuple<int, int, int, int, bool>(3, 1, 10,     600, true);

        }

        public Mission GetMission(int level, Location startLocation, List<Location> previousLocations)
        {
            var mission = new Mission();

            for (int i = 0; i < (LevelsDifficulty[level].Item1); i++)
            {
                var submission = SubMissionsFactory.Create(SubMissionType.ExactLocation, level, startLocation, LevelsDifficulty[level].Item2, LevelsDifficulty[level].Item3);

                mission.SubMissions.Add(submission);
            }

            return mission;
        }

        public string GetCityByCoordinates(double latitude, double longitude)
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
