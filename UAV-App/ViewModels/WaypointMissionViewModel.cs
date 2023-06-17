using DJI.WindowsSDK;
using DJI.WindowsSDK.Mission.Waypoint;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

            this.WaypointMission = new WaypointMission()
            {
                waypointCount = 0,
                maxFlightSpeed = 15,
                autoFlightSpeed = 10,
                finishedAction = WaypointMissionFinishedAction.NO_ACTION, //TODO set go home
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
                waypoints = new List<Waypoint>()
                {
                }
            };
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

                // if (WaypointMissionState.Equals(WaypointMissionState.))
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
                        CameraCommandHandler cameraCommandHandler = new CameraCommandHandler();
                        cameraCommandHandler.GetMostRecentPhoto();
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

        private LocationCoordinate2D _aircraftLocation = new LocationCoordinate2D() { latitude = 0, longitude = 0 };
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

        public void AddWaypoint(double lat, double lon)
        {

            geoPoints.Add(new LocationCoordinate2D() { latitude = lat, longitude = lon });

        }

        public void RemoveLastWaypoint()
        {

            if (geoPoints.Count > 0)
            {
                geoPoints.RemoveAt(geoPoints.Count - 1);
            }
        }

        public List<LocationCoordinate2D> getFoundAnimalPoints()
        {
            //TESTCODE
            var TempChaseAwayLoc = new List<LocationCoordinate2D>(chaseAwayGeoPoints);
            chaseAwayGeoPoints.Clear();
            return TempChaseAwayLoc;
        }

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

            SDKError err = SDKError.UNKNOWN;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(scoutMission);

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err != SDKError.NO_ERROR) return false;


            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err != SDKError.NO_ERROR) return false;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();

                if (err == SDKError.NO_ERROR)
                {
                    return true;
                }

                await Task.Delay(500);
            }
            return false;
        }

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

            SDKError err = SDKError.UNKNOWN;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(attackMission);

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err != SDKError.NO_ERROR) return false;


            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();

                if (err == SDKError.NO_ERROR)
                {
                    break;
                }

                await Task.Delay(500);
            }

            if (err != SDKError.NO_ERROR) return false;

            for (int i = 0; i < RETRY_AMOUNT; i++)
            {
                err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();

                if (err == SDKError.NO_ERROR)
                {
                    return true;
                }

                await Task.Delay(500);
            }
            return false;
        }

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

        public ICommand _loadMission;
        public ICommand LoadMission
        {
            get
            {
                if (_loadMission == null)
                {
                    _loadMission = new RelayCommand(async delegate ()
                    {
                        SDKError err1 = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(this.WaypointMission);
                        SDKError err2 = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();

                        var messageDialog = new MessageDialog(String.Format("SDK load mission: {0}, upload mission result", err1.ToString(), err2.ToString()));
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _loadMission;
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
                        missionGeoPoints = geoPoints;
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
