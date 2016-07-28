using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DocDbUtils;
using GameManager;
using GoogleApi.Entities.Common;
using Shared;
using Telegram.Bot;

namespace GameManagerWebApi.Controllers
{

    [RoutePrefix("api/game")]
    public class GameController : ApiController
    {
        private readonly TelegramBotClient _botClient;

        public enum UserState
        {
            None = 0,
            Checkin = 1,
            Go = 2,
        }

        // userId -> tuple<gameId, state>
        public static ConcurrentDictionary<string, Tuple<string, UserState>> States = new ConcurrentDictionary<string, Tuple<string, UserState>>();

        public static MissionController MissionController = new MissionController();

        public GameController()
        {
            _botClient = new TelegramBotClient("269182723:AAFP1qBAcnfnY0g9HHkw0a4jR69DmroR4Gg");
        }

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
        public async Task<HttpResponseMessage> Join(string gameId, [FromBody] TelegramUser user)
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


                var message = $"{user.Name} successfully joined FriendsGo!";
                await _botClient.SendTextMessageAsync(gameId, message);

                return new HttpResponseMessage() {Content = new StringContent(message) };
            }
            else
            {
                var message = $"{user.Name} already joined this FriendsGo group!";
                await _botClient.SendTextMessageAsync(gameId, message);

                Trace.TraceInformation($"{user.Name} already joined {gameId}!");
                return new HttpResponseMessage() { Content = new StringContent(message) } ;
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
            States[userId] = new Tuple<string, UserState>(gameId, UserState.Checkin);

            return $"State for user {userId} changed to {States[userId]}";
        }

        [HttpGet]
        [Route("{gameId}/mission")]
        public async Task<HttpResponseMessage> GetMission(string gameId)
        {
            var group = DocDbApi.GetGroupById(gameId);

            if (group != null)
            {
                Mission mission;

                if (group.GetCurrentMission() == null)
                {
                    mission = MissionController.GetMission(group.Level, group.StartLocation, new List<Location>() { group.StartLocation });

                    group.GeneratedMissions[group.Level] = mission;

                    await DocDbApi.UpdateGroup(group.TelegramId, group);
                }
                else
                {
                    mission = group.GetCurrentMission();
                }

                var message = $"Group {group.TelegramId} is on level {group.Level}. " + Environment.NewLine +
                              $"Your current missions are:" + Environment.NewLine +
                              $"{string.Join(Environment.NewLine, mission.SubMissions.Select(s => s.Description))}";

                await _botClient.SendTextMessageAsync(gameId, message);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(message)    
                };
            }

            throw new ArgumentException($"Group {gameId} does not exist!");
        }

        [HttpPost]
        [Route("location")]
        public async Task<HttpResponseMessage> Location([FromBody] UserLocation location)
        {
            Trace.TraceInformation("Location request");
            string message = string.Empty;
            var userId = location.UserId;

            if (States[userId] == null)
            {
                Trace.TraceInformation($"user {userId} is not on the list");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            if (States[userId].Item2 == UserState.Go)
            {
                Trace.TraceInformation($"user {userId} has go'ed the game");
                var groupId = States[userId].Item1;
                var group = DocDbApi.GetGroupById(groupId);

                group.StartLocation = location.ToLocation();

                await DocDbApi.UpdateGroup(group.TelegramId, group);

                message = $"{userId} has GO'ed the game in {group.TelegramId} group!";

                await _botClient.SendTextMessageAsync(groupId, message);
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
                        }

                        await DocDbApi.UpdateGroup(group.TelegramId, group);

                        await _botClient.SendTextMessageAsync(groupId, message);
                    }
                }
            }
            else
            {
                Trace.TraceInformation("The user status is incorrect");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var botResponse = new BotResponse(userId, States[userId].Item1, message);
            var reponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(botResponse);

            Trace.TraceInformation($"bot response: {reponseJson}");
            States[userId] = new Tuple<string, UserState>(string.Empty, UserState.None);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(reponseJson)
            };
        }

        [HttpGet]
        [Route("{gameId}/stat")]
        public async Task Stat(string gameId)
        {
            await _botClient.SendTextMessageAsync(gameId, "Hello test");
        }

        [HttpGet]
        [Route("{gameId}/globalStat")]
        public async Task GlobalStat(string gameId)
        {
            var topGroups = DocDbApi.GetTopGroups();
            Trace.TraceInformation($"bot response: topGroups : {topGroups}");
            await _botClient.SendTextMessageAsync(gameId, topGroups);
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
