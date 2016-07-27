using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            Trace.TraceInformation($"user {userId} changed his status to Go");
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

            if (DocDbApi.GetUserGroupById(user.Id, gameId) == null)
            {
                await DocDbApi.AddUserGroups(user.Id, gameId);
                Trace.TraceInformation($"{user.Name} successfully joined FriendsGo group {gameId}!");
                return $"{user.Name} successfully joined FriendsGo group {gameId}!";
            }
            else
            {
                Trace.TraceInformation($"{user.Name} already joined {gameId}!");
                return $"{user.Name} already joined {gameId}!";
            }
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
                    mission = group.GetCurrentMission();
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
            Trace.TraceInformation("Location request");
            string message = string.Empty;
            var userId = location.UserId;

            if (States[userId] == null)
            {
                Trace.TraceInformation($"user {userId} is not on the list");
                return "";
            }
            if (States[userId].Item2 == UserState.Go)
            {
                Trace.TraceInformation($"user {userId} has go'ed the game");
                var groupId = States[userId].Item1;
                var group = DocDbApi.GetGroupById(groupId);

                group.StartLocation = location.ToLocation();

                DocDbApi.UpdateGroup(group.TelegramId, group);

                message = $"{userId} has GO'ed the game in {group.TelegramId} group!";
            }
            else if (States[userId].Item2 == UserState.Checkin)
            {
                Trace.TraceInformation($"user {userId} has sent checkin location");
                var groupId = States[userId].Item1;
                var group = DocDbApi.GetGroupById(groupId);

                var mission = group.GetCurrentMission();

                if (mission != null)
                {
                    // for debuging:
                    //var validationResult = mission.ValidateLocation(location.ToLocation(), userId, debugMode: true);
                    var validationResult = mission.ValidateLocation(location.ToLocation(), userId);

                    if (validationResult)
                    {
                        message += $"Check-in successfull for game {States[userId].Item1}!"; ;

                        var completeRsult = mission.IsCompleted();

                        if (completeRsult)
                        {
                            message += Environment.NewLine + "Mission completed!";

                            group.Level += 1;
                            DocDbApi.UpdateGroup(group.TelegramId, group);
                        }

                    }
                }
            }
            else
            {
                Trace.TraceInformation("The user status is incorrect");
                throw new ArgumentException();
            }

            var botResponse = new BotResponse(userId, States[userId].Item1, message);
            var reponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(botResponse);

            Trace.TraceInformation($"bot response: {reponseJson}");
            States[userId] = new Tuple<string, UserState>(string.Empty, UserState.None);
            return reponseJson;
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

    public class BotResponse
    {
        public string UserId;
        public string GroupId;
        public string Message;

        public BotResponse(string userId, string groupId, string message)
        {
            UserId = userId;
            GroupId = groupId;
            Message = message;
        }
    }
}
