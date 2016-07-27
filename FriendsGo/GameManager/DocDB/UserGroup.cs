using System;
using System.Runtime.InteropServices;
using GameManager;

namespace Shared
{
    public class UserGroup : DocDbEntityBase
    {
        public string GroupId { get; set; }
        public string UserId { get; set; }

        public UserGroup(string groupId, string userId)
        {
            TelegramId = string.Format("{0}_{1}", userId, groupId);
            GroupId = groupId;
            UserId = userId;
        }
    }
}