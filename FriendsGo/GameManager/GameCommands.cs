using GoogleApi.Entities.Common;

namespace GameManager
{
    public interface IGameCommands
    {
        void JoinGame(string groupId, string userId);

        void StartGame(string groupId);

        string GetMission(string groupId);

        void CheckIn(string groupId, string userId, Location location);
    }
}
