using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameManager;
using GoogleApi.Entities.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class GameControllerTests
    {
        [TestMethod]
        public void MissionSerializationTest()
        {
            Mission m = new Mission();
            m.SubMissions.Add(SubMissionsFactory.Create(SubMissionType.ExactLocation, 1, new Location(11, 12), 1, 20, 20));

            string json = JsonConvert.SerializeObject(m, Formatting.Indented);

            m = JsonConvert.DeserializeObject<Mission>(json);

            Assert.IsTrue(m.SubMissions.First() is ExactLocationSubMission);
        }

    }
}
