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

        public async void onEnter()
        {
            List<LocationCoordinate2D> harmfullAnimalSpots = WaypointMissionViewModel.Instance.getFoundAnimalPoints();


            await WaypointMissionViewModel.Instance.startAttackMission(harmfullAnimalSpots);
        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.StartScoutPatrol == patrolEvent)
            {
                return new ScoutPatrolState();
            }

            return null;
        }
    }
}
