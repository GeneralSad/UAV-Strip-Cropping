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
    public class ScoutPatrolState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        private bool missionStarted;
        private bool missionExecuting;
        List<LocationCoordinate2D> spots;
        private int photoTargetWaypoint;
        public async void onEnter()
        {
            spots = WaypointMissionViewModel.Instance.getFirstLocations();
            missionStarted = false;
            missionExecuting = false;

            photoTargetWaypoint = 0;
        }

        public void onLeave()
        {
            //WaypointMissionViewModel.Instance.removeFirstLocations(3);
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
                missionStarted = await WaypointMissionViewModel.Instance.startScoutMission(spots);
                lastRanTime = System.DateTime.UtcNow;
            }
            else
            {
                if (!missionExecuting)
                {
                    var state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();

                    if (WaypointMissionState.EXECUTING == state)
                    {
                        Debug.WriteLine("scout executing done");
                        missionExecuting = true;
                    }
                }
                else
                {
                    if (System.DateTime.UtcNow - lastRanTime > timeout)
                    {
                        lastRanTime = System.DateTime.UtcNow;

                        /*              WaypointMission? mission = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
                                        if (mission == null) // get loaded mission returns null when the mission is done*/

                        WaypointMissionExecutionState? missionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLatestExecutionEvent();
                        if (!missionState.HasValue)
                        {
                            Debug.WriteLine("scout mission done");
                            WaypointMissionViewModel.Instance.WaypointMissionDone();
                            photoTargetWaypoint++;
                        }
                    }
                        
                }
            }
        }
    }
}
