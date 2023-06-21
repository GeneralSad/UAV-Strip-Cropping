using DJI.WindowsSDK;
using DJI.WindowsSDK.Mission.Waypoint;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UAV_App.Drone_Communication;
using UAV_App.Drone_Manager;
using UAV_App.Drone_Patrol;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace UAV_App.Pages
{
    class WaypointMissionViewModel : ViewModelBase
    {
        private static readonly WaypointMissionViewModel _singleton = new WaypointMissionViewModel();
        public static WaypointMissionViewModel Instance
        {
            get
            {
                return _singleton;
            }
        }

        const int RETRY_AMOUNT = 5;

        public List<LocationCoordinate2D> geoPoints { get; set; }

        public List<LocationCoordinate2D> missionGeoPoints = new List<LocationCoordinate2D>();
        private List<LocationCoordinate2D> chaseAwayGeoPoints = new List<LocationCoordinate2D>();

        private WaypointMissionViewModel()
        {
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StateChanged += WaypointMission_StateChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += WaypointMission_AircraftLocationChanged;
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ExecutionStateChanged += WaypointMission_ExecuteStateChanged;
            WaypointMissionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();

            geoPoints = new List<LocationCoordinate2D>();
        }

        /// <summary>
        /// returns a new waypoint with all of the correct settings
        /// </summary>
        /// <param name="latitude"> the lattitude of the waypoint</param>
        /// <param name="longitude"> the longitude of the waypoint</param>
        /// <param name="altitude"> the altitude of the waypoint</param>
        /// <param name="waypointActions"> waypoint actions, the default is no actions</param>
        /// <returns></returns>
        private Waypoint NewWaypoint(double latitude, double longitude, double altitude, List<WaypointAction> waypointActions = null)
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

        /// <summary>
        /// adds a new waypoint
        /// </summary>
        /// <param name="lat"> the latitude of the waypoint </param>
        /// <param name="lon"> the longitude of the waypoint </param>
        public void AddWaypoint(double lat, double lon)
        {

            geoPoints.Add(new LocationCoordinate2D() { latitude = lat, longitude = lon });

        }

        /// <summary>
        /// removes the last waypoint
        /// </summary>
        public void RemoveLastWaypoint()
        {

            if (geoPoints.Count > 0)
            {
                geoPoints.RemoveAt(geoPoints.Count - 1);
            }
        }

        /// <summary>
        /// returns all found locations of harmfull animals
        /// </summary>
        /// <returns>all found locations of harmfull animals</returns>
        public List<LocationCoordinate2D> getFoundAnimalPoints()
        {
            //TESTCODE
            var TempChaseAwayLoc = new List<LocationCoordinate2D>(chaseAwayGeoPoints);
            chaseAwayGeoPoints.Clear();
            return TempChaseAwayLoc;
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
                scoutMissionWaypoints.Add(NewWaypoint(loc.latitude, loc.longitude, 40, actions));
            }

            if (geoPoints.Count <= 0)
            { // if there are is only one geopoint the drone location is the first waypoint

                scoutMissionWaypoints.Insert(0, NewWaypoint(AircraftLocation.latitude, AircraftLocation.longitude, 40, actions));

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
            
            result = await LoadWaypointMission(scoutMission);

            if (!result) return false; // did the mission load correctly? if not return false

            result = await UploadWaypointMission();

            if (!result) return false; // did the mission upload correctly? if not return false

            result = await StartWaypointMission();

            if (!result) return false; // did the mission start correctly? if not return false

            return true;
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
                attackMissionWaypoints.Add(NewWaypoint(loc.latitude, loc.longitude, 40));
                attackMissionWaypoints.Add(NewWaypoint(loc.latitude, loc.longitude, 10));
                attackMissionWaypoints.Add(NewWaypoint(loc.latitude, loc.longitude, 40));
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
            
            result = await LoadWaypointMission(attackMission);
            if (!result) return false; // did the mission load correctly? if not return false

            result = await UploadWaypointMission();
            if (!result) return false; // did the mission upload correctly? if not return false

            result = await StartWaypointMission();
            if (!result) return false; // did the mission start correctly? if not return false

            return true;
        }

        /// <summary>
        /// loads the mission waypoints into the sdk
        /// </summary>
        /// <param name="mission"> the waypointmission to be loaded</param>
        /// <returns> boolean indicating if the task was succesfull</returns>
        private async Task<bool> LoadWaypointMission(WaypointMission mission)
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
                Console.WriteLine($"load mission error: {err.ToString()}");
                return false;
            }
        }

         private async Task<bool> UploadWaypointMission()
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
                Console.WriteLine($"upload mission error: {err.ToString()}");
                return false;
            }
        }


        /// <summary>
        /// function that calls the startmission method of the drone and retriest it if it fails
        /// </summary>
        /// <returns></returns>
         private async Task<bool> StartWaypointMission()
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
                Console.WriteLine($"start mission error: {err.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Retreives the first few locations of the mission geopoints.
        /// </summary>
        /// <returns> the first few locations of mission geopoints</returns>
        public List<LocationCoordinate2D> getFirstLocations()
        {
            List<LocationCoordinate2D> locations = new List<LocationCoordinate2D>();

            int itemAmount = missionGeoPoints.Count < 3 ? missionGeoPoints.Count : 3;
            for (int i = 0; i < itemAmount; i++)
            {
                locations.Add(missionGeoPoints[0]);
                missionGeoPoints.RemoveAt(0);

            }

            chaseAwayGeoPoints = missionGeoPoints;

            return locations;

        }


        public async void WaypointMission_AircraftLocationChanged(object sender, LocationCoordinate2D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftLocation = value.Value;
                }
            });
        }

        private async void WaypointMission_StateChanged(WaypointMissionHandler sender, WaypointMissionStateTransition? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                WaypointMissionState = value.HasValue ? value.Value.current : WaypointMissionState.UNKNOWN;


            });
        }

        private async void WaypointMission_ExecuteStateChanged(WaypointMissionHandler sender, WaypointMissionExecutionState? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                WaypointMissionExecuteState = value.HasValue ? value.Value.state : WaypointMissionExecuteState.UNKNOWN;

            });

            if (value.HasValue && value.Value.isExecutionFinish)
            {
                missionDone();
            }

        }

        private void missionDone()
        {

            if (chaseAwayGeoPoints.Count == 0)
            {
                if (missionGeoPoints.Count > 0)
                {
                    PatrolController.Instance.startScoutRouteEvent();
                }
                {
                    PatrolController.Instance.MissionDone();
                }

            }
            else
            {
                PatrolController.Instance.harmfullAnimalsFound();
            }

        }

        private WaypointMissionState _waypointMissionState;
        public WaypointMissionState WaypointMissionState
        {
            get
            {
                return _waypointMissionState;
            }
            set
            {
                _waypointMissionState = value;
                OnPropertyChanged(nameof(WaypointMissionState));
            }
        }

        private WaypointMissionExecuteState _waypointMissionExecuteState;
        public WaypointMissionExecuteState WaypointMissionExecuteState
        {
            get
            {
                return _waypointMissionExecuteState;
            }
            set
            {
                _waypointMissionExecuteState = value;
                OnPropertyChanged(nameof(WaypointMissionExecuteState));
            }
        }

        private ICommand _downloadMedia;
        public ICommand DownloadMedia
        {
            get
            {
                if (_downloadMedia == null)
                {
                    _downloadMedia = new RelayCommand(delegate ()
                    {
                        MediaHandler mediaHandler = new MediaHandler();
                        // TODO: File name
                        var name = "Photo.jpg";
                        mediaHandler.DownloadMostRecentPhoto(name);
                    }, delegate () { return true; });
                }
                return _downloadMedia;
            }
        }

        bool ShouldPause = true;

        public ICommand _pauseResumeMission;
        public ICommand PauseResumeMission
        {
            get
            {
                if (_pauseResumeMission == null)
                {
                    _pauseResumeMission = new RelayCommand(async delegate ()
                    {

                        if (ShouldPause)
                        {
                            var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).PauseMission();
                            var messageDialog = new MessageDialog(String.Format("Pause Result: {0}.", err.ToString()));
                            await messageDialog.ShowAsync();
                        }
                        else
                        {
                            var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ResumeMission();
                            var messageDialog = new MessageDialog(String.Format("Resume Result: {0}.", err.ToString()));
                            await messageDialog.ShowAsync();
                        }
                        ShouldPause = !ShouldPause;



                    }, delegate () { return true; });
                }
                return _pauseResumeMission;
            }
        }

        public ICommand _emergencyStop;
        public ICommand EmergencyStop
        {
            get
            {
                if (_emergencyStop == null)
                {
                    _emergencyStop = new RelayCommand(async delegate ()
                    {

                        var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();
                        var messageDialog = new MessageDialog(String.Format("stop mission Result: {0}.", err.ToString()));
                        await messageDialog.ShowAsync();


                    }, delegate () { return true; });
                }
                return _emergencyStop;
            }
        }

        public ICommand _goHome;
        public ICommand GoHome
        {
            get
            {
                if (_goHome == null)
                {
                    _goHome = new RelayCommand(async delegate ()
                    {
                        var err1 = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGoHomeHeightAsync(new IntMsg() { value = 40 });

                        var err2 = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartGoHomeAsync(); ;

                        var messageDialog = new MessageDialog(String.Format("stop mission Result: {0}.", err1.ToString() + " go home async result" + err2.ToString()));
                        await messageDialog.ShowAsync();


                    }, delegate () { return true; });
                }
                return _goHome;
            }
        }

        public ICommand _startLanding;
        public ICommand StartLanding
        {
            get
            {
                if (_goHome == null)
                {
                    _goHome = new RelayCommand(async delegate ()
                    {
                        var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync(); ;

                        var messageDialog = new MessageDialog(String.Format("Start auto landing: {0}.", err.ToString()));
                        await messageDialog.ShowAsync();


                    }, delegate () { return true; });
                }
                return _goHome;
            }
        }

        public ICommand _startTakeOff;
        public ICommand StartTakeOff
        {
            get
            {
                if (_startTakeOff == null)
                {
                    _startTakeOff = new RelayCommand(async delegate ()
                    {
                        var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
                        var messageDialog = new MessageDialog(String.Format("Start take off: {0}.", err.ToString()));
                        await messageDialog.ShowAsync();


                    }, delegate () { return true; });
                }
                return _startTakeOff;
            }
        }

        private WaypointMission _waypointMission;
        public WaypointMission WaypointMission
        {
            get { return _waypointMission; }
            set
            {
                _waypointMission = value;
                OnPropertyChanged(nameof(WaypointMission));
            }
        }

        private LocationCoordinate2D _aircraftLocation;
        public LocationCoordinate2D AircraftLocation
        {
            get
            {
                return _aircraftLocation;
            }
            set
            {
                _aircraftLocation = value;
                OnPropertyChanged(nameof(AircraftLocation));
            }
        }

        public ICommand _missionDone;
        public ICommand MissionDone
        {
            get
            {
                if (_missionDone == null)
                {
                    _missionDone = new RelayCommand(async delegate ()
                    {
                        missionDone();
                    }, delegate () { return true; });
                }
                return _missionDone;
            }
        }
        public ICommand _setGroundStationModeEnabled;
        public ICommand SetGroundStationModeEnabled
        {
            get
            {
                if (_setGroundStationModeEnabled == null)
                {
                    _setGroundStationModeEnabled = new RelayCommand(async delegate ()
                    {
                        SDKError err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGroundStationModeEnabledAsync(new BoolMsg() { value = true });
                        var messageDialog = new MessageDialog(String.Format("Set GroundStationMode Enabled: {0}", err.ToString()));
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _setGroundStationModeEnabled;
            }
        }

        public ICommand _startMission;
        public ICommand StartMission
        {
            get
            {
                if (_startMission == null)
                {
                    _startMission = new RelayCommand(async delegate ()
                    {
                        missionGeoPoints = new List<LocationCoordinate2D>(geoPoints);
                        PatrolController.Instance.startScoutRouteEvent();
                    }, delegate () { return true; });
                }
                return _startMission;
            }
        }

        public ICommand _startAttackMission;
        public ICommand StartAttackMission
        {
            get
            {
                if (_startAttackMission == null)
                {
                    _startAttackMission = new RelayCommand(async delegate ()
                    {
                        await startAttackMission(geoPoints);
                    }, delegate () { return true; });
                }
                return _startAttackMission;
            }
        }
    }
}
