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

        public bool ValidateLocation(Location loc)
        {
            bool validated = false;
            foreach (var subMission in SubMissions)
            {
                if (subMission.ValidateLocation(loc))
                {
                    validated = true;
                    break;
                }
            }
            return validated;

        }

        public bool IsCompleted()
        {
            bool completed = true;
            foreach (var subMission in SubMissions)
            {
                if (!subMission.IsCompleted())
                {
                    completed = false;
                }
            }
            return completed;
        }
    }

    public static class SubMissionsFactory
    {

        public static SubMission Create(SubMissionType type, int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration)
        {
            switch (type)
            {
                case SubMissionType.ExactLocation:
                    return new ExactLocationSubMission(level, startLocation, numberCheckInRequired, meterRadius, checkInCycleDuration);
                case SubMissionType.CityLocation:
                    return new CityLocationSubMission(level, startLocation, numberCheckInRequired, checkInCycleDuration);
                default:
                        throw new ArgumentException();
            }
        }
    }

    public abstract class SubMission
    {
        protected const string ApiKey = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8";

        public string Description;

        public abstract bool ValidateLocation(Location loc);

        public abstract bool IsCompleted();


    }


    public class ExactLocationSubMission : SubMission
    {
        public int NumberOfPlayers;
        
        //for in process validation
        private int _checkedInCount;
        private int _checkInCycleDuration;

        public Location ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission(int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration)
        {
            NumberOfPlayers = numberCheckInRequired;
            _checkedInCount = 0;
            _checkInCycleDuration = checkInCycleDuration;

            var placesRequest = new PlacesNearBySearchRequest()
            {
                Key = ApiKey,
                Sensor = true,
                Language = "en",
                Location = startLocation,
                Radius = meterRadius,
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

        public override bool ValidateLocation(Location loc)
        {
            //if location meets creteria
            _checkedInCount++;
            return true;
            //else false;
        }

        public override bool IsCompleted()
        {
            if (_checkedInCount == NumberOfPlayers)
            {
                return true;
            }
            return false;
        }
    }

    public class CityLocationSubMission : SubMission
    {
        public int NumberOfPlayers;

        //for in process validation
        private int _checkedInCount;
        private int _checkInCycleDuration;
        private string _city;

        private  List<string> Cities = new List<string>() {"Haifa", "Jerusalem", "BeerSheva"};

        public Location ExactLocation;

        public TimeSpan Duration;

        public CityLocationSubMission(int level, Location startLocation, int numberCheckInRequired,
            int checkInCycleDuration)
        {
           
            NumberOfPlayers = numberCheckInRequired;
            _checkedInCount = 0;
            _checkInCycleDuration = checkInCycleDuration;

            Random rand = new Random();
            int cityRand = rand.Next(0, Cities.Count - 1);
            _city = Cities[cityRand];

            Description = string.Format("Your mission: {0} players have to checkin to {1}!",
                NumberOfPlayers, _city);


        }

        public override bool ValidateLocation(Location loc)
        {
            //if location meets creteria
            _checkedInCount++;
            return true;
            //else false;
        }

        public override bool IsCompleted()
        {
            if (_checkedInCount == NumberOfPlayers)
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
