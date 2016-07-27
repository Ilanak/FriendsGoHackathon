using System;
using System.Collections.Generic;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;

namespace GameManager
{
    public class Mission 
    {
        public Mission()
        {
            SubMissions = new List<SubMission>();
        }

        public List<SubMission> SubMissions;

        public bool validateLocation(Location loc)
        {
            bool validated = false;
            foreach (var subMission in SubMissions)
            {
                if (subMission.validateLocation(loc))
                {
                    validated = true;
                    break;
                }
            }
            return validated;

        }

        public bool isCompleted()
        {
            bool completed = true;
            foreach (var subMission in SubMissions)
            {
                if (!subMission.isCompleted())
                {
                    completed = false;
                }
            }
            return completed;
        }
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
        protected const int MAX_PLAYERS_AMOUNT = 100;

        public Dictionary<int, int>  PlayersAmount = new Dictionary<int, int>()
        {
            { 1, 1}, {2,1} , {3,2} , {4,2} ,{ 5, 3}, {6,3} , {7,3} , {8,3}
        };
 

        protected const string ApiKey = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8";

        public string Description;

        public abstract bool validateLocation(Location loc);

        public abstract bool isCompleted();


    }


    public class ExactLocationSubMission : SubMission
    {
        private int NumberOfPlayers;
        
        //for in process validation
        private int checkedInCount;

        private List<bool> checkedIn = new List<bool>(MAX_PLAYERS_AMOUNT);

        public Location ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission(int level, Location startLocation)
        {
            NumberOfPlayers = PlayersAmount[level];
            checkedInCount = 0;

            var placesRequest = new PlacesNearBySearchRequest()
            {
                Key = ApiKey,
                Sensor = true,
                Language = "en",
                Location = startLocation,
                Radius = level * 50,
                Keyword = "*",
                Types = new List<SearchPlaceType>()
                {
                    SearchPlaceType.BAR, SearchPlaceType.FOOD, SearchPlaceType.CLOTHING_STORE, SearchPlaceType.RESTAURANT,
                    SearchPlaceType.GYM, SearchPlaceType.CAFE, SearchPlaceType.UNIVERSITY, SearchPlaceType.SCHOOL, SearchPlaceType.MOVIE_THEATER
                }
            };

            var response = GooglePlaces.NearBySearch.Query(placesRequest);

            var location = response.Results.FirstOrDefault(l => l.Photos != null);
            if (location != null)
            {
                ExactLocation = location.Geometry.Location;

                Description = string.Format("Your mission: {0} players have to checkin to {1}. It is at {2}!",
                    NumberOfPlayers, location.Name, location.Vicinity);

            }


            Duration = TimeSpan.MaxValue;
        }

        public override bool validateLocation(Location loc)
        {
            //if location meets creteria
            checkedInCount++;
            return true;
            //else false;
        }

        public override bool isCompleted()
        {
            if (checkedInCount == NumberOfPlayers)
            {
                return true;
            }
            return false;
        }
    }

    public enum SubMissionType
    {
        Default = 0,
        ExactLocation = 1,
        CityLocation = 2,
        CountryLocation = 3
    }
    
}
