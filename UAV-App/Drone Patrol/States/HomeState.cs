using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{
    public class HomeState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        private bool landingStarted;
        List<LocationCoordinate2D> spots;
        public async void onEnter()
        {

        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            switch (patrolEvent)
            {
                case PatrolEvent.StartScoutPatrol: 
                      case PatrolEvent.LandingDone: 
                    return new IdleState();
            }

            return null;
        }

        System.DateTime lastRanTime;
        TimeSpan timeout = TimeSpan.FromSeconds(1);


        public async Task run()
        {

            if (!landingStarted)
            {
              landingStarted = await WaypointMissionViewModel.Instance.goHome();
            } else if (System.DateTime.UtcNow - lastRanTime > timeout)
            {
                lastRanTime = System.DateTime.UtcNow;

                ResultValue<FCGoHomeStateMsg?> resultValue = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetGoHomeStateAsync();
                FCGoHomeStateMsg? homeState = resultValue.value;

                if (homeState == null && homeState.HasValue) // get loaded mission returns null when the mission is done
                {
                   if (FCGoHomeState.COMPLETED ==  homeState.Value.value)
                    {
                        HandleEvent(PatrolEvent.LandingDone);
                    }
                    
                }
            }
        }
    }
}
