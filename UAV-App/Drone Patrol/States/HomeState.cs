using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Manager;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{
    public class HomeState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        /// <summary>
        /// State returns the drone to the home position. Upon completion transfers active state to idle state.
        /// </summary>
        public HomeState() {}

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
              landingStarted = await goHome();

                PatrolController.Instance.landDoneEvent();
            } 
        }

        
        /// <summary>
        /// Sends the drone home
        /// </summary>
        /// <returns> Bool indicating if the home request was succesfully received</returns>
        public async Task<bool> goHome()
        {
            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartGoHomeAsync();

            if (err == SDKError.NO_ERROR)
            {
                return true;
            }
            else
            {
                Console.WriteLine($"go home error: {err}");
                return false;
            }
        }
    }
}
