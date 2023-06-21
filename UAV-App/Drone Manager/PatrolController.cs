using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Drone_Patrol;
using UAV_App.Pages;

namespace UAV_App.Drone_Manager
{

    /// <summary>
    /// State machine responsible for handling all of the logic of the code
    /// </summary>
    public class PatrolController : IPatrolMessage
    {
        private PatrolStateMachine patrolStateMachine;

        private static readonly PatrolController _singleton = new PatrolController();
        public static PatrolController Instance
        {
            get
            {
                return _singleton;
            }
        }
                
        const int RETRY_AMOUNT = 5;

        private PatrolController()
        {
            init();

            this.patrolStateMachine = new PatrolStateMachine();
        }

        private void init()
        {
            DJISDKManager.Instance.ComponentManager.GetWiFiHandler(0, 0).ConnectionChanged += ConnectionChanged;
        }

        public void droneResponseHandler()
        {

        }

        public async void ConnectionChanged(object sender, BoolMsg? changed)
        {
            if (changed != null && changed.HasValue && changed.Value.value)
            {
                ResultValue<BoolMsg?> result = await DJISDKManager.Instance.ComponentManager.GetWiFiHandler(0, 0).GetConnectionAsync().ConfigureAwait(false);

                if (result.value != null && result.value.HasValue)
                {
                    if (result.value.Value.value)
                    {
                        // WIFI CONNECTION
                        this.patrolStateMachine.ConnectionGained();
                    }
                    else
                    {
                        // NO CONNECTION
                        this.patrolStateMachine.ConnectionLost();
                    }
                }
            }

        }

                /// <summary>
        /// loads the mission waypoints into the sdk
        /// </summary>
        /// <param name="mission"> the waypointmission to be loaded</param>
        /// <returns> boolean indicating if the task was succesfull</returns>
        public static async Task<bool> LoadWaypointMission(WaypointMission mission)
        {
            SDKError err = SDKError.UNKNOWN;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(mission);

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err == SDKError.NO_ERROR)
            {
                return true;
            }
            else
            {                
                Debug.WriteLine($"load mission error:  {err} + state {DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState()}");
                return false;
            }
        }

        
        /// <summary>
        /// Uploads the stored mission to the drone
        /// </summary>
        /// <param name="mission"> the waypointmission to be loaded</param>
        /// <returns> boolean indicating if the task was succesfull</returns>
         public static async Task<bool> UploadWaypointMission()
        {
            SDKError err = SDKError.UNKNOWN;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err == SDKError.NO_ERROR)
            {
                return true;
            }
            else
            {
                Debug.WriteLine($"upload mission error: {err} + state {DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState()}");
                return false;
            }
        }


        /// <summary>
        /// function that calls the startmission method of the drone and retriest it if it fails
        /// </summary>
        /// <returns></returns>
         public static async Task<bool> StartWaypointMission()
        {
            SDKError err = SDKError.UNKNOWN;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err == SDKError.NO_ERROR)
            {
                return true;
            }
            else
            {                
                Debug.WriteLine($"start mission error:  {err} + state {DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState()}");
                return false;
            }
        }

                /// <summary>
        /// function that calls the stopmission method of the drone and retriest it if it fails
        /// </summary>
        /// <returns> bool indicating if cancel was succesfull</returns>
         public static async Task<bool> StopMission()
        {
            SDKError err = SDKError.UNKNOWN;
            int retryAmount = 5;

            for (int i = 0; i < retryAmount; i++)
            {
                err =  await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err == SDKError.NO_ERROR)
            {
                return true;
            }
            else
            {                
                Debug.WriteLine($"start mission error:  {err} + state {DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState()}");
                return false;
            }
        }

        
        /// <summary>
        /// returns a new waypoint with all of the correct settings
        /// </summary>
        /// <param name="latitude"> the lattitude of the waypoint</param>
        /// <param name="longitude"> the longitude of the waypoint</param>
        /// <param name="altitude"> the altitude of the waypoint</param>
        /// <param name="waypointActions"> waypoint actions, the default is no actions</param>
        /// <returns></returns>
        public static Waypoint NewWaypoint(double latitude, double longitude, double altitude, List<WaypointAction> waypointActions = null)
        {
            if (waypointActions == null)
            {
                waypointActions = new List<WaypointAction>();
            }

            Waypoint waypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = latitude, longitude = longitude },
                altitude = altitude,
                gimbalPitch = -90,
                turnMode = WaypointTurnMode.CLOCKWISE,
                heading = 0,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                speed = 0,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = waypointActions
            };
            return waypoint;
        }

        internal void EmergencyStopEvent()
        {
            this.patrolStateMachine.EmergencyStop();
        }

        internal void startScoutRouteEvent()
        {
            this.patrolStateMachine.StartScoutPatrol();
        }

        internal void harmfullAnimalsFound()
        {
            this.patrolStateMachine.ExpellAnimals();
        }

        internal void MissionDone()
        {
            this.patrolStateMachine.MissionDone();
        }

        internal void landDoneEvent()
        {
          this.patrolStateMachine.LandDone();
        }

        public async Task run()
        {
            await this.patrolStateMachine.run();
        }
    }
}
