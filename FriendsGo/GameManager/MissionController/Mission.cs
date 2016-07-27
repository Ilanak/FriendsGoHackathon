using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameManager
{
    
    public class Mission
    {
        public List<Location> ValidatedLocations;
        public List<SubMission> SubMissions;

        public Mission()
        {
            SubMissions = new List<SubMission>();
            ValidatedLocations = new List<Location>();
        }


        public bool ValidateLocation(Location loc, string userId, bool debugMode = false)
        {
            bool validated = false;
            if (SubMissions.Any(subMission => subMission.ValidateLocation(loc, userId, debugMode)))
            {
                validated = true;
                ValidatedLocations.Add(loc);
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

    [JsonConverter(typeof(SubMissionConverter))]
    public abstract class SubMission
    {
        protected const string ApiKey = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8";

        public string Description;

        public SubMissionType SubType;

        public abstract bool ValidateLocation(Location userLocation, string userId, bool debugMode);

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

        protected virtual bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            return true;
        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            //message users that time for compliting check in is over - start again?
            _checkedInCount = 0;
            checkIns.Clear();
        }


        public override bool ValidateLocation(Location userLocation, string userId, bool debugMode = false)
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
            if (!checkIns.ContainsKey(userId) && ValidateLocation(userLocation))
            {
                checkIns[userId] = true;
                _checkedInCount ++;
                return true;
            }
            else if (debugMode)
            {
                Trace.TraceInformation("debug mode");
                return true;
            }

            return false;
        }

        public override bool IsCompleted()
        {
            if (_checkedInCount == _numberOfPlayers)
            {
//                _timer.Enabled = false;
                return true;
            }
            return false;
        }
    }


    public class ExactLocationSubMission : SubMissionBase
    {
        public int NumberOfPlayers;

        public Location _exactLocation; 
        //for in process validation
        private int _checkedInCount;
        private int _checkInCycleDuration;

        public Location ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission() : base(0, 0)
        {
        }

        public ExactLocationSubMission(int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration) : base(numberCheckInRequired, checkInCycleDuration)
        {
            SubType = SubMissionType.ExactLocation;
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
                _exactLocation = location.Geometry.Location;

                Description = $"Your mission: {NumberOfPlayers} players have to checkin to {location.Name}. It is at {location.Vicinity}!";
            }

            Duration = TimeSpan.MaxValue;
        }

        protected override bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            var maxDistanceAllowed = 50;
            //if location meets creteria
            var distanceInMeters = DistanceAlgorithm.DistanceBetweenPlacesInMeters(userLocation, _exactLocation);

            Trace.TraceInformation($"ValidateLocation of exact location sub mission. distance between user location and" +
                                   $"expected location is {distanceInMeters}");

            if (distanceInMeters <= maxDistanceAllowed)
            {
                _checkedInCount++;
                return true;
            }
            else if (debugMode)
            {
                Trace.TraceInformation("debug mode");
                return true;
            }

            return false;
        }
    }

    public class DistanceAlgorithm
    {
        const double PIx = 3.141592653589793;
        const double RADIO = 6378.16;

        /// <summary>
        /// This class cannot be instantiated.
        /// </summary>
        private DistanceAlgorithm() { }

        /// <summary>
        /// Convert degrees to Radians
        /// </summary>
        /// <param name="x">Degrees</param>
        /// <returns>The equivalent in radians</returns>
        public static double Radians(double x)
        {
            return x * PIx / 180;
        }

        /// <summary>
        /// Calculate the distance between two places.
        /// </summary>
        public static double DistanceBetweenPlacesInMeters(
            Location location1,
            Location location2)
        {
            var lon1 = location1.Longitude;
            var lat1 = location1.Latitude;
            var lon2 = location2.Longitude;
            var lat2 = location2.Latitude;

            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (angle*RADIO) * 1000;
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

        public CityLocationSubMission() : base(0,0)
        {
        }

        public CityLocationSubMission(int level, Location startLocation, int numberCheckInRequired,
            int checkInCycleDuration) : base (numberCheckInRequired, checkInCycleDuration)
        {
            SubType = SubMissionType.CityLocation;
            NumberOfPlayers = numberCheckInRequired;
            _checkedInCount = 0;
            _checkInCycleDuration = checkInCycleDuration;

            Random rand = new Random();
            int cityRand = rand.Next(0, Cities.Count - 1);
            _city = Cities[cityRand];

            Description = string.Format("Your mission: {0} players have to checkin to {1}!",
                NumberOfPlayers, _city);


        }

        protected override bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            //if location meets creteria
            var userCity = MissionController.GetCityByCoordinates(userLocation.Latitude, userLocation.Longitude);
            if (userCity.Equals(_city, StringComparison.InvariantCultureIgnoreCase))
            {
                Trace.TraceInformation("User city check-in was validated successfully");
                _checkedInCount++;
                return true;
            }
            else if (debugMode)
            {
                Trace.TraceInformation("debug mode");
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

    public class SubMissionConverter : JsonCreationConverter<SubMission>
    {
        protected override SubMission Create(Type objectType, JObject jObject)
        {
            var type = jObject["SubType"].ToString();
            if (type == "1")
            {
                return new ExactLocationSubMission();
            }
            else if (type == "2")
            {
                return new CityLocationSubMission();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }


        public override bool CanWrite { get { return false; } }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
