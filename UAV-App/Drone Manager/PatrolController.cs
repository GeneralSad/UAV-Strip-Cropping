using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Manager
{
    public class PatrolController : IPatrolMessage
    {
        public PatrolController() 
        {
            init();
        }

        private void init()
        {

        }

        public void droneResponseHandler()
        {

        }

        public void cancelAndReturnToStartingPointEvent()
        {
            throw new NotImplementedException();
        }

        public void continueRouteEvent()
        {
            throw new NotImplementedException();
        }

        public void emergencyStopEvent()
        {
            throw new NotImplementedException();
        }

        public void landEvent()
        {
            throw new NotImplementedException();
        }

        public void startRouteEvent()
        {
            throw new NotImplementedException();
        }
    }
}
