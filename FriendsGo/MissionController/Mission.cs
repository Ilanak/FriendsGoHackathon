using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.NearBy.Request;
using GoogleApi.Entities.Places.Search.NearBy.Response;
using GoogleApi.Entities.Places.Search.Radar.Request;
using GoogleApi.Entities.Places.Search.Radar.Response;

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
                        return new ExactLocationSubMission(level, startLocation);
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

        public ExactLocationSubMission(int level, Location startLocation)
        {
            NumberOfPlayers = level;

            var placesRequest = new PlacesRadarSearchRequest()
            {
                Key = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8",
                Sensor = true,
                Language = "en",
                Location = startLocation,
                Radius = 100,
                Keyword = "*"
            };

            var response = GooglePlaces.RadarSearch.Query(placesRequest);

            var firstOrDefault = response.Results.FirstOrDefault();
            if (firstOrDefault != null)
                ExactLocation = firstOrDefault.Geometry.Location;

            Duration = TimeSpan.MaxValue;
        }
    }

    public enum SubMissionType
    {
        Default = 0,
        ExactLocation = 1,
    }
    
}
