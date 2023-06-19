using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Movement;
using UAV_App.Drone_Patrol;
using UAV_App.Pages;

namespace UAV_App.Drone_Manager
{
    public class PatrolController : IPatrolMessage
    {
        private PatrolStateMachine patrolStateMachine;
        private DroneMovementStateMachine droneMovementStateMachine;

        private static readonly PatrolController _singleton = new PatrolController();
        public static PatrolController Instance
        {
            get
            {
                return _singleton;
            }
        }

        private PatrolController()
        {
            init();

            this.patrolStateMachine = new PatrolStateMachine();
        }



        private void init()
        {
            DJISDKManager.Instance.ComponentManager.GetWiFiHandler(0, 0).ConnectionChanged += ConnectionChanged;
        }

        public void droneResponseHandler()
        {

        }

        public async void ConnectionChanged(object sender, BoolMsg? changed)
        {
            if (changed != null && changed.HasValue && changed.Value.value)
            {
                ResultValue<BoolMsg?> result = await DJISDKManager.Instance.ComponentManager.GetWiFiHandler(0, 0).GetConnectionAsync().ConfigureAwait(false);

                if (result.value != null && result.value.HasValue)
                {
                    if (result.value.Value.value)
                    {
                        // WIFI CONNECTION
                        this.patrolStateMachine.ConnectionGained();
                    }
                    else
                    {
                        // NO CONNECTION
                        this.patrolStateMachine.ConnectionLost();
                    }
                }
            }

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

        public void startScoutRouteEvent()
        {
            this.patrolStateMachine.StartScoutPatrol();
        }


        public void harmfullAnimalsFound()
        {
            this.patrolStateMachine.ExpellAnimals();
        }

        public void MissionDone()
        {
            this.patrolStateMachine.MissionDone();
        }
    }
}
