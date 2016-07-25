using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MissionController
{
    public interface IMissionController
    {
        Mission GetMission(int level, Location startLocation, List<Location> previousLocations);


    }

    public class MissionController : IMissionController
    {
        public Mission GetMission(int level, Location startLocation, List<Location> previousLocations)
        {
            throw new NotImplementedException();
        }
    }
}
