using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{
    public class ExpelAnimalsState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        public void onEnter()
        {
            List<Waypoint> harmfullAnimalSpots = WaypointMissionViewModel.Instance.getFoundAnimalPoints();

            WaypointMissionViewModel.Instance.startAttackMission(harmfullAnimalSpots).Wait();
        }

        public void onLeave()
        {
        }

        public IPatrolState run(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.ExpellDone == patrolEvent)
            {
                return new FollowingRouteState();
            }

            return null;
        }
    }
}
