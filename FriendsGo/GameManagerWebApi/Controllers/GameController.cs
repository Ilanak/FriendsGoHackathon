using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DocDbUtils;
using GameManager;
using GoogleApi.Entities.Common;
using Shared;

namespace GameManagerWebApi.Controllers
{

    [RoutePrefix("api/game")]
    public class GameController : ApiController 
    {

        public enum UserState
        {
            None = 0,
            Checkin = 1,
            Go = 2,
        }

        // userId -> tuple<gameId, state>
        public static ConcurrentDictionary<string, Tuple<string, UserState>> States = new ConcurrentDictionary<string, Tuple<string, UserState>>();

        public static MissionController MissionController = new MissionController();

        [HttpPost]
        [Route("{gameId}/go/{userId}")]
        public string Go(string gameId, string userId)
        {
            States[userId] = new Tuple<string, UserState>(gameId, UserState.Go);
            return $"State for user {userId} changed to {States[userId]}";
        }

        [HttpPost]
        [Route("{gameId}/join")]
        public async Task<string> Join(string gameId, [FromBody] TelegramUser user)
        {
            var group = DocDbApi.GetGroupById(gameId);

            if (group == null)
            {
                await DocDbApi.CreateGroup(new Group(gameId, null));
            }

            var telegramUser = DocDbApi.GetUserById(user.Id);

            if (telegramUser == null)
            {
                await DocDbApi.CreateUser(new BotUser(user.Id, user.Name));
            }

            // TODO: Connect user id to game

            return $"{user.Name} successfully joined FriendsGo group {gameId}!";
        }

        [HttpPost]
        [Route("cancel/{userId}")]
        public string Cancel(string userId)
        {
            States[userId] = new Tuple<string, UserState>(string.Empty, UserState.None);
            return "Operation canceled.";
        }

        [HttpPost]
        [Route("{gameId}/checkin/{userId}")]
        public string CheckIn(string gameId, string userId)
        {
            States[userId] = new Tuple<string, UserState>(gameId, UserState.Go);

            return $"State for user {userId} changed to {States[userId]}";
        }

        [HttpGet]
        [Route("{gameId}/mission")]
        public string GetMission(string gameId)
        {
            var group = DocDbApi.GetGroupById(gameId);

            if (group != null)
            {
                Mission mission;

                if (group.GetCurrentMission() == null)
                {
                    mission = MissionController.GetMission(group.Level, group.StartLocation, new List<Location>() {});

                    group.GeneratedMissions[group.Level] = mission;

                    DocDbApi.UpdateGroup(group.TelegramId, group);
                }
                else
                {
                    mission = group.GeneratedMissions[group.Level];
                }
                
                return $"Group {group.TelegramId} is on level {group.Level}. " + Environment.NewLine +
                           $"Your current missions are:" + Environment.NewLine +
                           $"{string.Join(Environment.NewLine, mission.SubMissions.Select(s => s.Description))}";
            }

            throw new ArgumentException($"Group {gameId} does not exist!");
        }

        [HttpPost]
        [Route("location")]
        public string Location([FromBody] UserLocation location)
        {
            string result = string.Empty;
            var userId = location.UserId;

            if (States[userId] == null)
            {
                return "";
            }
            else if (States[userId].Item2 == UserState.Go)
            {
                var groupId = States[userId].Item1;
                var group = DocDbApi.GetGroupById(groupId);

                group.StartLocation = location.ToLocation();

                DocDbApi.UpdateGroup(group.TelegramId, group);

                result = $"{userId} has GO'ed the game in {group.TelegramId} group!";
            }
            else if (States[userId].Item2 == UserState.Checkin)
            {
                var groupId = States[userId].Item1;
                var group = DocDbApi.GetGroupById(groupId);

                var mission = group.GetCurrentMission();

                if (mission != null)
                {
                    var validationResult = mission.ValidateLocation(location.ToLocation());

                    if (validationResult)
                    {
                        result += $"Check-in successfull for game {States[userId].Item1}!"; ;

                        var completeRsult = mission.IsCompleted();

                        if (completeRsult)
                        {
                            result += Environment.NewLine + "Mission completed!";
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }

            States[userId] = new Tuple<string, UserState>(string.Empty, UserState.None);
            return result;
        }

        [HttpGet]
        [Route("{gameId}/stat")]
        public void Stat(string groupId)
        {
            throw new NotImplementedException();
        }
    }

    public class TelegramUser
    {
        public string Name;
        public string Id;
    }

    public class UserLocation
    {
        public string UserId;
        public string Latitude;
        public string Longitude;

        public Location ToLocation()
        {
            return new Location(Convert.ToDouble(Latitude), Convert.ToDouble(Longitude));
        }
    }
}
