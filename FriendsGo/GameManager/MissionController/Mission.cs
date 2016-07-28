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
        public List<SubMission> SubMissions;

        public Mission()
        {
            SubMissions = new List<SubMission>();
        }


        public bool ValidateLocation(Location loc, string userId, bool debugMode = false)
        {
            bool validated = false;
            if (SubMissions.Any(subMission => subMission.ValidateLocation(loc, userId, debugMode)))
            {
                validated = true;
            }
            return validated;

        }
        public string MissionStatus()
        {
            string message = String.Empty;

            foreach (var subMission in SubMissions)
            {
                message += $"{subMission.LocationDescription}:  {subMission.CheckIns.Count()} / {subMission.NumberOfPlayers} checked in!" + Environment.NewLine;

            }
            return message;
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
        public string LocationDescription;

        public SubMissionType SubType;

        public abstract bool ValidateLocation(Location userLocation, string userId, bool debugMode);

        public abstract bool IsCompleted();

        public Dictionary<string, Location> CheckIns;
        public int NumberOfPlayers;
        public int CheckInCycleDuration;
        public TimeSpan Duration;

    }

    public class SubMissionBase : SubMission
    {
        protected  static Random rand = new Random(); 

        public SubMissionBase()
        {
            
        }
        
        public SubMissionBase(int numPlayers, int checkInCycleDuration)
        {
            CheckIns = new Dictionary<string, Location>();
            NumberOfPlayers = numPlayers;
            CheckInCycleDuration = checkInCycleDuration;
        }

        protected virtual bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            return true;
        }
        

        public override bool ValidateLocation(Location userLocation, string userId, bool debugMode = false)
        {
            if (IsCompleted())
            {
                return false;
            }
            if (!CheckIns.ContainsKey(userId) && ValidateLocation(userLocation))
            {
                CheckIns[userId] = userLocation;
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
            if (CheckIns.Count >= NumberOfPlayers)
            {
                return true;
            }
            return false;
        }
    }


    public class ExactLocationSubMission : SubMissionBase
    {
        public Location ExactLocation;


        public ExactLocationSubMission()
        {
        }

        public ExactLocationSubMission(int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration) : base(numberCheckInRequired, checkInCycleDuration)
        {
            SubType = SubMissionType.ExactLocation;
           
            while (true)
            {
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
                        SearchPlaceType.BAR,
                        SearchPlaceType.FOOD,
                        SearchPlaceType.CLOTHING_STORE,
                        SearchPlaceType.RESTAURANT,
                        SearchPlaceType.GYM,
                        SearchPlaceType.CAFE,
                        SearchPlaceType.UNIVERSITY,
                        SearchPlaceType.SCHOOL,
                        SearchPlaceType.MOVIE_THEATER
                    }
                };

                var response = GooglePlaces.NearBySearch.Query(placesRequest);

                if (response.Results.Count() > 0)
                {

                    int index = (rand.Next(0, response.Results.Count()));
                    var location = response.Results.ElementAt(index);

                    ExactLocation = location.Geometry.Location;

                    Description =
                        $"{NumberOfPlayers} player(s) need to checkin to {location.Name}. It is at {location.Vicinity}!";
                    LocationDescription = location.Name;

                    Duration = TimeSpan.MaxValue;

                    return;
                }

                meterRadius += 50;
            } 

        }

        protected override bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            var maxDistanceAllowed = 500;
            
            //if location meets creteria
            var distanceInMeters = DistanceAlgorithm.DistanceBetweenPlacesInMeters(userLocation, ExactLocation);

            Trace.TraceInformation($"ValidateLocation of exact location sub mission. distance between user location and" +
                                   $"expected location is {distanceInMeters}");

            if (distanceInMeters <= maxDistanceAllowed)
            {                
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
        public string City;

        private  List<string> Cities = new List<string>() {"Haifa", "Jerusalem", "BeerSheva"};

        public Location ExactLocation;

        public TimeSpan Duration;

        public CityLocationSubMission() 
        {
        }

        public CityLocationSubMission(int level, Location startLocation, int numberCheckInRequired,
            int checkInCycleDuration) : base (numberCheckInRequired, checkInCycleDuration)
        {
            SubType = SubMissionType.CityLocation;
            
            Random rand = new Random();
            int cityRand = rand.Next(0, Cities.Count - 1);
            City = Cities[cityRand];

            Description = string.Format("Your mission: {0} players have to checkin to {1}!", NumberOfPlayers, City);
            LocationDescription = City;
        }

        protected override bool ValidateLocation(Location userLocation, bool debugMode = false)
        {
            //if location meets creteria
            var userCity = MissionController.GetCityByCoordinates(userLocation.Latitude, userLocation.Longitude);
            if (userCity.Equals(City, StringComparison.InvariantCultureIgnoreCase))
            {
                Trace.TraceInformation("User city check-in was validated successfully");
                return true;
            }

            Trace.TraceInformation($"Expected city was: {City} while the user sent {userCity} location");
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
            //var numberOfPlayers = jObject["NumberOfPlayers"].ToObject<int>();

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
