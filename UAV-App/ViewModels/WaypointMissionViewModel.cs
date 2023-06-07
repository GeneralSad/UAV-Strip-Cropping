using DJI.WindowsSDK;
using DJI.WindowsSDK.Mission.Waypoint;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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

        private WaypointMissionViewModel()
        {
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).IsSimulatorStartedChanged += WaypointMission_IsSimulatorStartedChanged;
            DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StateChanged += WaypointMission_StateChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += WaypointMission_AltitudeChanged; ;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += WaypointMission_AircraftLocationChanged; ;
            WaypointMissionState = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).GetCurrentState();

            this.WaypointMission = new WaypointMission()
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

        private async void WaypointMission_AltitudeChanged(object sender, DoubleMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftAltitude = value.Value.value;
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

        private async void WaypointMission_IsSimulatorStartedChanged(object sender, BoolMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsSimulatorStart = value.HasValue && value.Value.value;
            });
        }

        public String SimulatorLatitude { set; get; }
        public String SimulatorLongitude { set; get; }
        public String SimulatorSatelliteCount { set; get; }
        bool _isSimulatorStart = false;
        public bool IsSimulatorStart
        {
            get
            {
                return _isSimulatorStart;
            }
            set
            {
                _isSimulatorStart = value;
                OnPropertyChanged(nameof(IsSimulatorStart));
                OnPropertyChanged(nameof(SimulatorState));
            }
        }
        public String SimulatorState
        {
            get
            {
                return _isSimulatorStart ? "Open" : "Close";
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


        private double _aircraftAltitude = 0;
        public double AircraftAltitude
        {
            get
            {
                return _aircraftAltitude;
            }
            set
            {
                _aircraftAltitude = value;
                OnPropertyChanged(nameof(AircraftAltitude));
            }
        }
        public ICommand _startSimulator;
        public ICommand StartSimulator
        {
            get
            {
                if (_startSimulator == null)
                {
                    _startSimulator = new RelayCommand(async delegate ()
                    {
                        try
                        {
                            var latitude = Convert.ToDouble(SimulatorLatitude);
                            var longitude = Convert.ToDouble(SimulatorLongitude);
                            var satelliteCount = Convert.ToInt32(SimulatorSatelliteCount);

                            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartSimulatorAsync(new SimulatorInitializationSettings
                            {
                                latitude = latitude,
                                longitude = longitude,
                                satelliteCount = satelliteCount
                            });
                            var messageDialog = new MessageDialog(String.Format("Start Simulator Result: {0}.", err.ToString()));
                            await messageDialog.ShowAsync();
                        }
                        catch
                        {
                            var messageDialog = new MessageDialog("Format error!");
                            await messageDialog.ShowAsync();
                        }
                    }, delegate () { return true; });
                }
                return _startSimulator;
            }
        }

        public ICommand _initWaypointMission;
        public ICommand InitWaypointMission
        {
            get
            {
                if (_initWaypointMission == null)
                {
                    _initWaypointMission = new RelayCommand(delegate ()
                    {
                        double nowLat = AircraftLocation.latitude;
                        double nowLng = AircraftLocation.longitude;
                        WaypointMission mission = new WaypointMission()
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
                            waypoints = new List<Waypoint>()
                            {
                                InitDumpWaypoint(nowLat+0.0001, nowLng+0.00015),
                                InitDumpWaypoint(nowLat+0.0001, nowLng-0.00015),
                                InitDumpWaypoint(nowLat-0.0001, nowLng-0.00015),
                                InitDumpWaypoint(nowLat-0.0001, nowLng+0.00015),
                            }
                        };
                        WaypointMission = mission;
                    }, delegate () { return true; });
                }
                return _initWaypointMission;
            }
        }

        public ICommand _stopSimulator;
        public ICommand StopSimulator
        {
            get
            {
                if (_stopSimulator == null)
                {
                    _stopSimulator = new RelayCommand(async delegate ()
                    {
                        var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StopSimulatorAsync();
                        var messageDialog = new MessageDialog(String.Format("Stop Simulator Result: {0}.", err.ToString()));
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _stopSimulator;
            }
        }


        bool switchBool = false;

        public ICommand _pauseResumeMission;
        public ICommand PauseResumeMission
        {
            get
            {
                if (_pauseResumeMission == null)
                {
                    _pauseResumeMission = new RelayCommand(async delegate ()
                    {

                        if (switchBool)
                        {
                            var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).PauseMission();
                            var messageDialog = new MessageDialog(String.Format("Stop Simulator Result: {0}.", err.ToString()));
                            await messageDialog.ShowAsync();
                        }
                        else
                        {
                            var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).ResumeMission();
                            var messageDialog = new MessageDialog(String.Format("Stop Simulator Result: {0}.", err.ToString()));
                            await messageDialog.ShowAsync();
                        }
                        switchBool = !switchBool;



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

                        var err1 = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();  
 

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
                        var err1 = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGoHomeHeightAsync(new IntMsg() { value = 40});
             
                        var err2 = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartGoHomeAsync();;  


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

        private Waypoint InitDumpWaypoint(double latitude, double longitude)
        {
            Waypoint waypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = latitude, longitude = longitude },
                altitude = 40,
                gimbalPitch = -30,
                turnMode = WaypointTurnMode.CLOCKWISE,
                heading = 0,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                speed = 0,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = new List<WaypointAction>()
            };
            return waypoint;
        }



        public void AddWaypoint(double lat, double lon)
        {
            double nowLat = AircraftLocation.latitude;
            double nowLng = AircraftLocation.longitude;
            WaypointMission waypoints = WaypointMission;

            waypoints.waypoints.Add(InitDumpWaypoint(lat, lon));

            WaypointMission = waypoints;

        }

        public void RemoveLastWaypoint()
        {
            WaypointMission waypoints = WaypointMission;

            if (waypoints.waypoints.Count > 0)
            {
                waypoints.waypoints.RemoveAt(waypoints.waypoints.Count - 1);

                WaypointMission = waypoints;
            }
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
                    SDKError err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(this.WaypointMission);
                    var messageDialog = new MessageDialog(String.Format("SDK load mission: {0}", err.ToString()));
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
    public ICommand _uploadMission;
    public ICommand UploadMission
    {
        get
        {
            if (_uploadMission == null)
            {
                _uploadMission = new RelayCommand(async delegate ()
                {
                    SDKError err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();
                    var messageDialog = new MessageDialog(String.Format("Upload mission to aircraft: {0}", err.ToString()));
                    await messageDialog.ShowAsync();
                }, delegate () { return true; });
            }
            return _uploadMission;
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
                    var err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();
                    var messageDialog = new MessageDialog(String.Format("Start mission: {0}", err.ToString()));
                    await messageDialog.ShowAsync();
                }, delegate () { return true; });
            }
            return _startMission;
        }
    }
}
}
