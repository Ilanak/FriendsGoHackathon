using System;
using System.Collections.Generic;
using GoogleApi.Entities.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameManager;
using Shared;

namespace UnitTests
{
    [TestClass]
    public class MissionControllerTests
    {

        private const string userId = "testUserId";
        private const string userName = "testUserName";
        private const string groupId = "testGroupId";

        [TestInitialize]
        public void initDocDbConnection()
        {
            DocDbUtils.DocDbApi.InitDocDbConnection();
        }
        [TestMethod]
        public void Test()
        {
            var controller = new MissionController();

            var result = controller.GetMission(1, new Location(32.158278, 34.808194), new List<Location>() {});

            Assert.AreEqual(result.SubMissions.Count, 1);
        }


        [TestMethod]
        public void GroupQueryTest()
        {
            var group = DocDbUtils.DocDbApi.GetGroupById(groupId);
            Assert.AreEqual(group.TelegramId, groupId);
        }

        [TestMethod]
        public void CreatUserTest()
        {
            BotUser usr = new BotUser(userId, userName);
            DocDbUtils.DocDbApi.CreateUser(usr).Wait();
        }

        [TestMethod]
        public void CreateGroupTest()
        {
            Group grp = new Group(groupId, null);
            DocDbUtils.DocDbApi.CreateGroup(grp).Wait();
        }

        [TestMethod]
        public void UserQueryTest()
        {
            var user = DocDbUtils.DocDbApi.GetUserById(userId);
            Assert.AreEqual(user.UserName, userName);
            Assert.AreEqual(user.TelegramId, userId);
        }

        [TestMethod]
        public void UpdateGroupTest()
        {
            var grp = DocDbUtils.DocDbApi.GetGroupById(groupId);
            if (grp != null)
            {
                grp.Level = 1;
                DocDbUtils.DocDbApi.UpdateGroup(grp.TelegramId, grp);
            }

            grp = DocDbUtils.DocDbApi.GetGroupById(groupId);

            Assert.AreEqual(grp.Level, 1);
        }

        [TestMethod]
        public void CreateCollectionTest()
        {
            DocDbUtils.DocDbApi.CreateCollection("UsersGroups");
        }

        [TestMethod]
        public void DeleteGroup()
        {
            DocDbUtils.DocDbApi.DeleteGroup(groupId);
        }

        [TestMethod]
        public void DeleteUser()
        {
            DocDbUtils.DocDbApi.DeleteUser(userId);
        }

        [TestMethod]
        public void AddGroupUser()
        {
            DocDbUtils.DocDbApi.AddUserGroups(groupId, userId).Wait();
        }
    }
}
