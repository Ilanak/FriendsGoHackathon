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

            var originalSubmission = SubMissionsFactory.Create(SubMissionType.ExactLocation, 5, new Location(11, 12), 7, 20, 50);
            originalSubmission.Description = "1234  abcd";
            originalSubmission.CheckIns["assaf"] = new Location(33, 44);

            m.SubMissions.Add(originalSubmission);
            
            string json = JsonConvert.SerializeObject(m, Formatting.Indented);

            m = JsonConvert.DeserializeObject<Mission>(json);

            var submission = m.SubMissions.First() as ExactLocationSubMission;


            //Assert.AreEqual(submission, originalSubmission);
        }

    }
}
