using System;
using System.Collections.Generic;
using GoogleApi.Entities.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionController;

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
        public void TestDocDbGroupQuery()
        {
            var group = DocDbUtils.DocDbApi.getGroupById("448c6202-f5d4-4513-8510-134e89b6dbab");
            Assert.AreEqual(group.Id, "448c6202-f5d4-4513-8510-134e89b6dbab");
        }

        [TestMethod]
        public void TestDocDbUserQuery()
        {
            var user = DocDbUtils.DocDbApi.getUserById("00000000-0000-0000-0000-000000000000");
            Assert.AreEqual(user.UserName, "testUser");
        }
    }
}
