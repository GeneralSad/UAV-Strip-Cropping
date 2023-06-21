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
    public class ExpelAnimalsState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        /// <summary>
        /// this state retreives all currently found harmfull spots. 
        /// Then it will call the mission responsible for executing the expell methods.
        /// Currently that means a waypoint mission that will move to the spot at 40m, rapidly descent to 5m and then go back to 40m and repeat
        /// </summary>
        public ExpelAnimalsState() {}

        private List<LocationCoordinate2D> harmfullAnimalSpots;
        private bool missionStarted;

        public async void onEnter()
        {
           harmfullAnimalSpots = WaypointMissionViewModel.Instance.getFoundAnimalPoints();
           missionStarted = false;
        }

        public void onLeave()
        {
        }

        public IPatrolState HandleEvent(PatrolEvent patrolEvent)
        {
            if (PatrolEvent.StartScoutPatrol == patrolEvent)
            {
                return new ScoutPatrolState();
            } else if (PatrolEvent.MissionDone == patrolEvent)
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
              missionStarted = await startAttackMission(harmfullAnimalSpots);

            } 
            else if (System.DateTime.UtcNow - lastRanTime > timeout)
            {
                lastRanTime = System.DateTime.UtcNow;

               // WaypointMission? mission = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLoadedMission();
                 // if (mission == null) // get loaded mission returns null when the mission is done

                WaypointMissionExecutionState? missionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetLatestExecutionEvent();
                if (missionState.HasValue && missionState.Value.isExecutionFinish) 
                {
                    WaypointMissionViewModel.Instance.WaypointMissionDone();
                }
            }
        }

                /// <summary>
        /// creates, loads, uploads and starts a scout mission. 
        /// A scout mission is a mission with waypoints at 40m high, where pictures are taken at every location
        /// </summary>
        /// <param name="geoPoints"> The geopoints where the waypoints for the scout mission will be started</param>
        /// <returns> a boolean indicating if the operation was succesfull</returns>
        public async Task<bool> startAttackMission(List<LocationCoordinate2D> geoPoints)
        {
            List<Waypoint> attackMissionWaypoints = new List<Waypoint>();

            foreach (LocationCoordinate2D loc in geoPoints)
            {
                attackMissionWaypoints.Add(PatrolController.Instance.NewWaypoint(loc.latitude, loc.longitude, 40));
                attackMissionWaypoints.Add(PatrolController.Instance.NewWaypoint(loc.latitude, loc.longitude, 10));
                attackMissionWaypoints.Add(PatrolController.Instance.NewWaypoint(loc.latitude, loc.longitude, 40));
            }

            WaypointMission attackMission = new WaypointMission()
            {
                waypointCount = 0,
                maxFlightSpeed = 15,
                autoFlightSpeed = 10,
                finishedAction = WaypointMissionFinishedAction.NO_ACTION,
                headingMode = WaypointMissionHeadingMode.AUTO,
                flightPathMode = WaypointMissionFlightPathMode.NORMAL,
                gotoFirstWaypointMode = WaypointMissionGotoFirstWaypointMode.SAFELY,
                exitMissionOnRCSignalLostEnabled = false,
                pointOfInterest = new LocationCoordinate2D()
                {
                    latitude = 0,
                    longitude = 0
                },
                gimbalPitchRotationEnabled = true,
                repeatTimes = 0,
                missionID = 0,
                waypoints = attackMissionWaypoints
            };

            bool result;
            
            result = await PatrolController.Instance.LoadWaypointMission(attackMission);
            if (!result) return false; // did the mission load correctly? if not return false

            result = await PatrolController.Instance.UploadWaypointMission();
            if (!result) return false; // did the mission upload correctly? if not return false

            result = await PatrolController.Instance.StartWaypointMission();
            if (!result) return false; // did the mission start correctly? if not return false

            return true;
        }
    }
}
