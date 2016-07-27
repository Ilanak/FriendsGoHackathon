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
            var group = DocDbUtils.DocDbApi.GetGroupById("testGroup");
            Assert.AreEqual(group.TelegramId, "testGroup");
        }

        [TestMethod]
        public void CreatUserTest()
        {
            BotUser usr = new BotUser("testId", "TestName");
            DocDbUtils.DocDbApi.CreateUser(usr).Wait();
        }

        [TestMethod]
        public void CreateGroupTest()
        {
            Group grp = new Group("testGroup", null);
            DocDbUtils.DocDbApi.CreateGroup(grp).Wait();
        }

        [TestMethod]
        public void UserQueryTest()
        {
            var user = DocDbUtils.DocDbApi.GetUserById("testId");
            Assert.AreEqual(user.UserName, "testUser");
        }

        [TestMethod]
        public void UpdateGroupTest()
        {
            var grp = DocDbUtils.DocDbApi.GetGroupById("testGroup");
            if (grp != null)
            {
                grp.Level = 1;
                DocDbUtils.DocDbApi.UpdateGroup(grp.TelegramId, grp);
            }
            //Assert.AreEqual(user.UserName, "testUser");
        }

        [TestMethod]
        public void CreateCollectionTest()
        {
            DocDbUtils.DocDbApi.CreateCollection("UsersGroups");
        }

        [TestMethod]
        public void DeleteGroup()
        {
            DocDbUtils.DocDbApi.DeleteGroup("testGroup");
        }
    }
}
