using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Communication;
using UAV_App.Drone_Manager;
using UAV_App.Pages;

namespace UAV_App.Drone_Patrol.States
{

    public class ScoutPatrolState : IPatrolState
    {
        public ParentState getParent()
        {
            return ParentState.PATROUILLING;
        }

        WaypointMissionState[] executingStates = { WaypointMissionState.EXECUTING, WaypointMissionState.DISCONNECTED, WaypointMissionState.UPLOADING };


        /// <summary>
        /// State responsible for scouting an area. this is moving from waypoint to waypoint to take pictures of each waypoint
        /// </summary>
        public ScoutPatrolState()
        { }

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
                missionStarted = await startScoutMission(spots);
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

                        WaypointMissionState state = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();
                        
                        if (executingStates.Contains(state))
                        {
                            Debug.WriteLine("scout mission done");
                            WaypointMissionViewModel.Instance.WaypointMissionDone();
                        }

                        if (missionState.Value.isWaypointReached && missionState.Value.targetWaypointIndex == photoTargetWaypoint)
                        {
                            MediaHandler mediaHandler = new MediaHandler();
                            mediaHandler.DownloadMostRecentPhoto(WaypointMissionViewModel.Instance.AircraftLocation);
                            Debug.WriteLine(spots[photoTargetWaypoint]);
                            photoTargetWaypoint++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// creates, loads, uploads and starts a scout mission. 
        /// A scout mission is a mission with waypoints at 40m high, where pictures are taken at every location
        /// </summary>
        /// <param name="geoPoints"> The geopoints where the waypoints for the scout mission will be started</param>
        /// <returns> a boolean indicating if the operation was succesfull</returns>
        public async Task<bool> startScoutMission(List<LocationCoordinate2D> geoPoints)
        {
            List<Waypoint> scoutMissionWaypoints = new List<Waypoint>();
            List<WaypointAction> actions = new List<WaypointAction>() {
                new WaypointAction() { actionType = WaypointActionType.START_TAKE_PHOTO },
                new WaypointAction() { actionType = WaypointActionType.STAY, actionParam = 5000 },
            };

            foreach (LocationCoordinate2D loc in geoPoints)
            {
                scoutMissionWaypoints.Add(PatrolController.NewWaypoint(loc.latitude, loc.longitude, 40, actions));
            }

            if (geoPoints.Count == 1)
            { // if there are is only one geopoint the drone location is the first waypoint
                var aircraftLocation = WaypointMissionViewModel.Instance.AircraftLocation;
                scoutMissionWaypoints.Insert(0, PatrolController.NewWaypoint(aircraftLocation.latitude, aircraftLocation.longitude, 40, actions));
            };

            WaypointMission scoutMission = new WaypointMission()
            {
                waypointCount = 0,
                maxFlightSpeed = 15,
                autoFlightSpeed = 10,
                finishedAction = WaypointMissionFinishedAction.NO_ACTION,
                headingMode = WaypointMissionHeadingMode.AUTO,
                flightPathMode = WaypointMissionFlightPathMode.NORMAL,
                gotoFirstWaypointMode = WaypointMissionGotoFirstWaypointMode.SAFELY,
                exitMissionOnRCSignalLostEnabled = false,
                gimbalPitchRotationEnabled = true,
                repeatTimes = 0,
                missionID = 0,
                waypoints = scoutMissionWaypoints
            };

            bool result;

            result = await PatrolController.LoadWaypointMission(scoutMission);

            if (!result) return false; // did the mission load correctly? if not return false

            result = await PatrolController.UploadWaypointMission();

            if (!result) return false; // did the mission upload correctly? if not return false

            result = await PatrolController.StartWaypointMission();

            if (!result) return false; // did the mission start correctly? if not return false

            return true;
        }
    }
}

