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

        private bool missionStarted;
        List<LocationCoordinate2D> spots;
        private int photoTargetWaypoint;
        public async void onEnter()
        {
            spots = WaypointMissionViewModel.Instance.getFirstLocations(3);
            missionStarted = false;

            photoTargetWaypoint = 0;
        }

        public void onLeave()
        {
            WaypointMissionViewModel.Instance.removeFirstLocations(3);
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
            } 
            else if (System.DateTime.UtcNow - lastRanTime > timeout)
            {
                lastRanTime = System.DateTime.UtcNow;

/*              WaypointMission? mission = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
                if (mission == null) // get loaded mission returns null when the mission is done*/
               
                WaypointMissionExecutionState? missionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLatestExecutionEvent();
                if (missionState.HasValue) 
                {
                    if (missionState.Value.isExecutionFinish) {
                        WaypointMissionViewModel.Instance.WaypointMissionDone();
                        return;
                    }

                    if (missionState.Value.isWaypointReached && missionState.Value.targetWaypointIndex == photoTargetWaypoint)
                    {
                        WaypointMissionViewModel.Instance.DownloadWaypointFoto(photoTargetWaypoint);

                        photoTargetWaypoint++;
                    }

                }
            }
        }
    }
}
