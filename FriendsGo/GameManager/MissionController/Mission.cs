using System;
using System.Collections.Generic;
using System.Linq;
using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    [JsonConverter(typeof(SubMissionConverter))]
    public abstract class SubMission
    {
        protected const int MaxPlayersAmount = 100;

        protected const string ApiKey = "AIzaSyA5t84tAgn_fgRCXM1ROaOjcEfRiMG4AZ8";

        public string Description;

        public SubMissionType SubType;

        public abstract bool ValidateLocation(Location loc);

        public abstract bool IsCompleted();


    }

    
    public class ExactLocationSubMission : SubMission
    {
        public int NumberOfPlayers;
        
        //for in process validation
        private int _checkedInCount;
        private int _checkInCycleDuration;


        private List<bool> checkedIn = new List<bool>(MaxPlayersAmount);

        public Location ExactLocation;

        public TimeSpan Duration;

        public ExactLocationSubMission()
        {
        }

        public ExactLocationSubMission(int level, Location startLocation, int numberCheckInRequired, int meterRadius, int checkInCycleDuration)
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


        private List<bool> checkedIn = new List<bool>(MaxPlayersAmount);

        public Location ExactLocation;

        public TimeSpan Duration;

        public CityLocationSubMission()
        {
        }

        public CityLocationSubMission(int level, Location startLocation, int numberCheckInRequired,
            int checkInCycleDuration)
        {
            SubType = SubMissionType.CityLocation;
            NumberOfPlayers = numberCheckInRequired;
            _checkedInCount = 0;
            _checkInCycleDuration = checkInCycleDuration;

            _city = "Haifa";

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
