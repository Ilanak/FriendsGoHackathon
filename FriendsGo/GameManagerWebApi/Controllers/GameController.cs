using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Services.Protocols;
using DocDbUtils;
using Shared;

namespace GameManagerWeb.Controllers
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
            
        [HttpPost]
        [Route("{gameId}/go/{userId}")]
        public string Go(string gameId, string userId)
        {
            States[userId] = new Tuple<string, UserState>(gameId, UserState.Go);
            return "";
        }

        [HttpPost]
        [Route("{gameId}/join")]
        public string Join(string gameId, [FromBody] TelegramUser user)
        {
            var group = DocDbApi.getGroupById(gameId);

            if (group == null)
            {
                //DocDbApi.CreateGroup(new Group(gameId, null));
            }

            return "";
        }

        [HttpPost]
        [Route("cancel/{userId}")]
        public string Cancel(string userId)
        {
            States[userId] = new Tuple<string, UserState>(string.Empty, UserState.None);
            return "";
        }

        [HttpPost]
        [Route("{gameId}/checkin/{userId}")]
        public string CheckIn(string gameId, string userId)
        {
            States[userId] = new Tuple<string, UserState>(gameId, UserState.Go);

            return "";
        }

        [HttpGet]
        [Route("{gameId}/mission")]
        public string GetMission(string gameId)
        {
            var group = DocDbApi.getGroupById(gameId);

            if (group != null)
            {
                return $"Group {group.Id} is on level {group.Level}. Your current mission is to go to BBB!";
            }

            throw new ArgumentException();
        }

        [HttpPost]
        [Route("location")]
        public string Location([FromBody] UserLocation location)
        {
            string result;
            var userId = location.UserId;
            if (States[userId].Item2 == UserState.Go)
            {
                result =  $"{userId} has GO'ed the game in {States[userId].Item1} group!";
            }
            else if (States[userId].Item2 == UserState.Checkin)
            {
                result =  $"{userId} has checked-in for game {States[userId].Item1}!";
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
            throw new System.NotImplementedException();
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
        public string Longtitude;
    }


}
