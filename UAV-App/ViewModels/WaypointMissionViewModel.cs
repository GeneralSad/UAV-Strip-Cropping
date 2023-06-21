using DJI.WindowsSDK;
using DJI.WindowsSDK.Mission.Waypoint;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        
        public void DownloadWaypointFoto(int currentWaypoint)
        {
           LocationCoordinate2D waypoint = missionGeoPoints[currentWaypoint];
            
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

                    // Testcode for chase away geopoints
                    chaseAwayGeoPoints = new List<LocationCoordinate2D>(locations);

                    return locations;

        }

        /// <summary>
        /// Checks if there are more missions to do, and updates patrolstate to which mission type is neccesary.
        /// if there are no more waypoints to handle ends the mission
        /// </summary>        
        public void WaypointMissionDone()
        {
            if (chaseAwayGeoPoints.Count == 0)
            {
                if (missionGeoPoints.Count > 0)
                {
                    PatrolController.Instance.startScoutRouteEvent();
                } else 
                {
                    PatrolController.Instance.MissionDone();
                }
            }
            else
            {
                PatrolController.Instance.harmfullAnimalsFound();
            }

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
                        LocationCoordinate2D location = new LocationCoordinate2D
                        {
                            latitude = 0.0,
                            longitude = 0.0
                        };
                        mediaHandler.DownloadMostRecentPhoto(location);
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
                        var err =  await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();
                        var messageDialog = new MessageDialog(String.Format("Emergency Result: {0}.", err.ToString()));
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
                        WaypointMissionDone();
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
                       // await startAttackMission(geoPoints);
                    }, delegate () { return true; });
                }
                return _startAttackMission;
            }
        }
    }
}
