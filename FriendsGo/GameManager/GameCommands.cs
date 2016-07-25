using GoogleApi.Entities.Common;

namespace GameManager
{
    public interface IGameCommands
    {
        void JoinGame(string groupId, string userId);

        void StartGame(string groupId);

        string GetMission(string groupId);

        void CheckIn(string groupId, string userId, Location location);

        bool IsGroupExists(string groupId);
    }

    public class GameCommands : IGameCommands
    {
        public bool IsGroupExists(string groupId)
        {
            return true;
        }

        public void JoinGame(string groupId, string userId)
        {
            throw new System.NotImplementedException();
        }

        public void StartGame(string groupId)
        {
            throw new System.NotImplementedException();
        }

        public string GetMission(string groupId)
        {
            throw new System.NotImplementedException();
        }

        public void CheckIn(string groupId, string userId, Location location)
        {
            throw new System.NotImplementedException();
        }
    }
}
