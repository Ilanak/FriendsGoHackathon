using System;
using System.Collections.Generic;
using GoogleApi.Entities.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionController;
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
            var controller = new MissionController.MissionController();

            var result = controller.GetMission(1, new Location(32.158278, 34.808194), new List<Location>() {});

            Assert.AreEqual(result.SubMissions.Count, 1);
        }


        [TestMethod]
        public void GroupQueryTest()
        {
            var group = DocDbUtils.DocDbApi.GetGroupById("448c6202-f5d4-4513-8510-134e89b6dbab");
            Assert.AreEqual(group.TelegramId, "448c6202-f5d4-4513-8510-134e89b6dbab");
        }

        [TestMethod]
        public void CreatUserTest()
        {
            BotUser usr = new BotUser("testId2");
            DocDbUtils.DocDbApi.CreateUser(usr).Wait();
        }

        [TestMethod]
        public void CreateGroupTest()
        {
            Group grp = new Group("TestGroupNull", null);
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
            var grp = DocDbUtils.DocDbApi.GetGroupById("TestGroup");
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
            DocDbUtils.DocDbApi.CreateCollection("UsersGroups").Wait();            
        }
    }
}
