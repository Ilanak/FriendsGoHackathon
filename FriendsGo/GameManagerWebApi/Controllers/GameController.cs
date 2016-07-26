using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Services.Protocols;
using Shared;

namespace GameManagerWeb.Controllers
{
  
    public interface IGameController
    {
        string Join(string groupId);

        string Start(string groupId);

        string GetMission(string groupId);

        void CheckIn(string groupId, string userId, double latitude, double longitude);

        void Stat(string groupId);
    }

    [RoutePrefix("api/game")]
    public class GameController : ApiController // , IGameController
    {
        [HttpPost]
        [Route("{gameId}/join")]
        public string Join(string gameId, [FromBody] TelegramUser user)
        {
            return $"{user.Name} has joined the game {gameId}!";
        }

        [HttpPost]
        [Route("{gameId}/start")]
        public string Start(string gameId)
        {
            return "Game started!";
        }

        [HttpGet]
        [Route("{gameId}/mission")]
        public string GetMission(string gameId)
        {
            return $"Group {gameId} is on level ''. Your current mission is to go to BBB!";
        }

        public void CheckIn(string groupId, string userId, double latitude, double longitude)
        {
            throw new System.NotImplementedException();
        }

        public void Stat(string groupId)
        {
            throw new System.NotImplementedException();
        }

        public bool IsGroupExists(string groupId)
        {
            var user = new BotUser("exampleId1");
            DocDbUtils.DocDbApi.CreateUser(user).Wait();
            return true;
        }
    }


    public class TelegramUser
    {
        public string Name;
    }


}
