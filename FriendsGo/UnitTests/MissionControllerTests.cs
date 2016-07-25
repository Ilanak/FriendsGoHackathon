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
        [TestMethod]
        public void Test()
        {
            var controller = new MissionController.MissionController();

            var result = controller.GetMission(1, new Location(32.158278, 34.808194), new List<Location>() {});

            Assert.AreEqual(result.SubMissions.Count, 1);
        }
    }
}
