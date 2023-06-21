using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Patrol;
using UAV_App.Pages;

namespace UAV_App.Drone_Manager
{

    /// <summary>
    /// State machine responsible for handling all of the logic of the code
    /// </summary>
    public class PatrolController : IPatrolMessage
    {
        private PatrolStateMachine patrolStateMachine;

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

        internal void EmergencyStopEvent()
        {
            this.patrolStateMachine.EmergencyStop();
        }

        internal void startScoutRouteEvent()
        {
            this.patrolStateMachine.StartScoutPatrol();
        }

        internal void harmfullAnimalsFound()
        {
            this.patrolStateMachine.ExpellAnimals();
        }

        internal void MissionDone()
        {
            this.patrolStateMachine.MissionDone();
        }

        public async Task run()
        {
            await this.patrolStateMachine.run();
        }

        internal void landDoneEvent()
        {
          this.patrolStateMachine.LandDone();
        }
    }
}
