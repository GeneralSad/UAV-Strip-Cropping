using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private List<LocationCoordinate2D> harmfullAnimalSpots;
        private bool missionStarted;
        private bool missionExecuting;

        public async void onEnter()
        {
            harmfullAnimalSpots = WaypointMissionViewModel.Instance.getFoundAnimalPoints();
            missionStarted = false;
            missionExecuting = false;
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
            else if (PatrolEvent.MissionDone == patrolEvent)
            {
                return new HomeState();
            }

            return null;
        }

        System.DateTime lastRanTime;
        TimeSpan timeout = TimeSpan.FromSeconds(1);

        public async Task run()
        {

            if (!missionStarted)
            {
                missionStarted = await WaypointMissionViewModel.Instance.startAttackMission(harmfullAnimalSpots);
                lastRanTime = System.DateTime.UtcNow;
            }
            else
            {
                if (!missionExecuting)
                {
                    var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();

                    if (WaypointMissionState.EXECUTING == state)
                    {
                        Debug.WriteLine("patrol executing done");
                        missionExecuting = true;
                    }

                }
                else
                {

                    if (System.DateTime.UtcNow - lastRanTime > timeout)
                    {
                        // WaypointMission? mission = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
                        // if (mission == null) // get loaded mission returns null when the mission is done

                        WaypointMissionExecutionState? missionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLatestExecutionEvent();
                        if (!missionState.HasValue) // latest execution event is set to null when mission completes
                        {
                            Debug.WriteLine("patrol mission done");
                            WaypointMissionViewModel.Instance.WaypointMissionDone();
                        }
                    }

                }

            }



        }
    }
}
