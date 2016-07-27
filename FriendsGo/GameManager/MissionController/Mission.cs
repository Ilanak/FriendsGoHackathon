using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;
using Newtonsoft.Json;

namespace GameManager
{
    [JsonConverter(typeof(UserConverter))]
    public class Mission
    {
        public List<Location> ValidatedLocations;
        public List<SubMission> SubMissions;

        public Mission()
        {
            SubMissions = new List<SubMission>();
            ValidatedLocations = new List<Location>();
        }


        public bool ValidateLocation(Location loc, string userId)
        {
            bool validated = false;
            foreach (var subMission in SubMissions)
            {
                if (subMission.ValidateLocation(loc, userId))
                {
                    validated = true;
                    ValidatedLocations.Add(loc);
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

        public abstract bool ValidateLocation(Location loc, string userId);

        public abstract bool IsCompleted();


    }

    public class SubMissionBase : SubMission
    {
        private Dictionary<string, bool> checkIns;
        private int _checkedInCount;
        private int _numberOfPlayers;
        private int _checkInCycleDuration;
        private static System.Timers.Timer _timer;

        public SubMissionBase(int numPlayers, int checkInCycleDuration)
        {
            checkIns = new Dictionary<string, bool>();
            _checkedInCount = 0;
            _numberOfPlayers = numPlayers;
            _checkInCycleDuration = checkInCycleDuration;
        }

        protected virtual bool ValidateLocation(Location loc)
        {
            return true;
        }


        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }


        public override bool ValidateLocation(Location loc, string userId)
        {
            //if first cehck in set clock!
            if (checkIns.Count == 0)
            {
//                _timer = new System.Timers.Timer();
//                // Hook up the Elapsed event for the timer.
//                _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
//                _timer.Interval = _checkedInCount * 1000 /*to seconds*/;
//                _timer.Enabled = true;
            }
            if (!checkIns.ContainsKey(userId) && ValidateLocation(loc))
            {
                checkIns[userId] = true;
                _checkedInCount ++;
                return true;
            }
            return false;
        }

        public override bool IsCompleted()
        {
            if (_checkedInCount == _numberOfPlayers)
            {
                return true;
            }
            return false;
        }
    }


    public class ExactLocationSubMission : SubMissionBase
    {
        public int NumberOfPlayers;
        
        //for in process validation
        private int _checkedInCount;
        private int _checkInCycleDuration;

        public Location ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission(int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration) : base (numberCheckInRequired, checkInCycleDuration)
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

        protected override bool ValidateLocation(Location loc)
        {
            //if location meets creteria
            _checkedInCount++;
            return true;
            //else false;
        }

    }

    public class CityLocationSubMission : SubMissionBase
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
            int checkInCycleDuration) : base (numberCheckInRequired, checkInCycleDuration)
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

        protected override bool ValidateLocation(Location loc)
        {
            //if location meets creteria
            var userCity = MissionController.GetCityByCoordinates(loc.Latitude, loc.Longitude);
            if (userCity.Equals(_city, StringComparison.InvariantCultureIgnoreCase))
            {
                Trace.TraceInformation("User city check-in was validated successfully");
                _checkedInCount++;
                return true;
            }


            Trace.TraceInformation($"Expected city was: {_city} while the user sent {userCity} location");
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

    public class UserConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
