using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Manager;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{
    public class EmergencyStopState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.NONE;
        }

        public void onEnter()
        {

        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.MissionStopped == patrolEvent)
            {
                return new IdleState();
            }

            return null;
        }


        public async Task run()
        {
             
            await PatrolController.StopMission();

            WaypointMissionState state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();

            if (WaypointMissionState.READY_TO_UPLOAD == state || WaypointMissionState.READY_TO_EXECUTE == state)
            {
              PatrolController.Instance.MissionDone(); 
            }
        }

    }
}
