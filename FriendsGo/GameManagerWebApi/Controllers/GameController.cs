using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Services.Protocols;

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
        [Route("{gameId:int}/join")]
        public string Join(int gameId, [FromBody] string userName)
        {
            return $"{userName} has joined the game {gameId}!";
        }

        [HttpPost]
        public string Start(string groupId)
        {
            return "Game started!";
        }

        public string GetMission(string groupId)
        {
            throw new System.NotImplementedException();
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
}
