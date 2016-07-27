using System;
using System.Collections.Generic;
using System.Linq;
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

            var submission = SubMissionsFactory.Create(SubMissionType.ExactLocation, level, startLocation);
            
            mission.SubMissions.Add(submission);

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
