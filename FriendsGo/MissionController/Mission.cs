using System;
using System.Collections.Generic;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;

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
        protected const string ApiKey = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8";
    }

    public class ExactLocationSubMission : SubMission
    {
        public int NumberOfPlayers;

        public Location  ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission(int level, Location startLocation)
        {
            NumberOfPlayers = level;

            var placesRequest = new PlacesNearBySearchRequest()
            {
                Key = ApiKey,
                Sensor = true,
                Language = "en",
                Location = startLocation,
                Radius = 100,
                Keyword = "*",
                Types = new List<SearchPlaceType>()
                {
                    SearchPlaceType.ATM, SearchPlaceType.BAR, SearchPlaceType.FOOD, SearchPlaceType.CLOTHING_STORE, SearchPlaceType.RESTAURANT,
                    SearchPlaceType.GYM, SearchPlaceType.CAFE, SearchPlaceType.BUS_STATION, SearchPlaceType.UNIVERSITY, SearchPlaceType.SCHOOL, SearchPlaceType.MOVIE_THEATER
                }
            };

            var response = GooglePlaces.NearBySearch.Query(placesRequest);

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
