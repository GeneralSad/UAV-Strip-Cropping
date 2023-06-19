using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{
    public class ScoutPatrolState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        public async void onEnter()
        {
            List<LocationCoordinate2D> spots = WaypointMissionViewModel.Instance.getFirstLocations();

            await WaypointMissionViewModel.Instance.startScoutMission(spots);
        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            switch (patrolEvent)
            {
                case PatrolEvent.StartScoutPatrol: 
                    return new ScoutPatrolState();
                      case PatrolEvent.ExpellAnimals: 
                    return new ExpelAnimalsState();
                      case PatrolEvent.MissionDone: 
                    return new IdleState();
            }

            return null;
        }
    }
}
