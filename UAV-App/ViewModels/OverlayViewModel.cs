using DJI.WindowsSDK;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UAV_App.Pages;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UAV_App.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {

        //Singleton to acces public viewmodel
        private static readonly OverlayViewModel _singleton = new OverlayViewModel();
        public static OverlayViewModel Instance
        {
            get
            {
                return _singleton;
            }
        }

        //Empty, can't add listeners when SDK hasn't started yet.
        private OverlayViewModel()
        {
        }

        //Add listeners to SDK events
        public void StartOverlay()
        {
            DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += BatteryPercentageChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SatelliteCountChanged += SatelliteCountChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += AircraftAltitudeChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += AircraftLocationChanged;

            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).IsHomeLocationSetChanged += AircraftHomeLocationChanged;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).VelocityChanged += AircraftVelocityChanged;
        }

        public async void BatteryPercentageChanged(object sender, IntMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    BatteryLevel = value.Value.value;
                }
            });
        }

        public async void SatelliteCountChanged(object sender, IntMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    SatelliteCount = value.Value.value;
                }
            });
        }

        public async void AircraftAltitudeChanged(object sender, DoubleMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftAltitude = value.Value.value;
                }
            });
        }

        //Update UI, limit decimals to a maximum of 6
        public async void AircraftLocationChanged(object sender, LocationCoordinate2D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftLongitude = Math.Round(value.Value.longitude, 6);
                    AircraftLatitude = Math.Round(value.Value.latitude, 6);
                }
            });
        }

        //Update Set Home button color to signify if a home point has been set
        public async void AircraftHomeLocationChanged(object sender, BoolMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    Windows.UI.Color color;
                    if (value.Value.value)
                    {
                        color = Colors.Green;
                    }
                    else
                    {
                        color = Colors.Red;
                    }
                    color.A = 100;
                    SetHomeColor = new SolidColorBrush(color);
                }
            });
        }

        //Update UI speed values
        //SDK returns speeds in North, East, Down
        //To display horizontal speed take absolute of north and east value and add them together
        //For vertical take absolute value of down
        public async void AircraftVelocityChanged(object sender, Velocity3D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    double horizontalSpeed = Math.Abs(value.Value.x) + Math.Abs(value.Value.y);
                    double verticalSpeed = Math.Abs(value.Value.z);
                    AircraftHorizontalSpeed = Math.Round(horizontalSpeed, 1);
                    AircraftVerticalSpeed = Math.Round(verticalSpeed, 1);
                }
            });
        }

        //Emergency stop logic
        public ICommand _emergencyStop;
        public ICommand EmergencyStop
        {
            get
            {
                if (_emergencyStop == null)
                {
                    _emergencyStop = new RelayCommand(delegate ()
                    {
                        Debug.WriteLine("Emergency");
                    }, delegate () { return true; });
                }
                return _emergencyStop;
            }
        }

        //Make the flight controller update the home location
        public ICommand _setHomePoint;
        public ICommand SetHomePoint
        {
            get
            {
                if (_setHomePoint == null)
                {
                    _setHomePoint = new RelayCommand(async delegate ()
                    {
                        if (DJISDKManager.Instance.SDKRegistrationResultCode == SDKError.NO_ERROR)
                        {
                            await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetHomeLocationUsingAircraftCurrentLocationAsync();
                        }
                    }, delegate () { return true; });
                }
                return _setHomePoint;
            }
        }

        //Swap location of the live feed
        public ICommand _feedTapped;
        public ICommand FeedTapped
        {
            get
            {
                if (_feedTapped == null)
                {
                    _feedTapped = new RelayCommand(delegate ()
                    {
                        if (SwapChainRow == 1)
                        {
                            SwapChainRow = 0;
                            SwapChainColumn = 0;
                            SwapChainWidth = 2;
                        }
                        else
                        {
                            SwapChainRow = 1;
                            SwapChainColumn = 1;
                            SwapChainWidth = 1;
                        }
                    }, delegate () { return true; });
                }
                return _feedTapped;
            }
        }

        private SolidColorBrush _setHomeColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 204, 204));
        public SolidColorBrush SetHomeColor
        {
            get
            {
                return _setHomeColor;
            }
            set
            {
                _setHomeColor = value;
                OnPropertyChanged(nameof(SetHomeColor));
            }
        }

        private int _batteryLevel = 0;
        public int BatteryLevel
        {
            get
            {
                return _batteryLevel;
            }
            set
            {
                _batteryLevel = value;
                OnPropertyChanged(nameof(BatteryLevel));
            }
        }

        private int _satelliteCount = 0;
        public int SatelliteCount
        {
            get
            {
                return _satelliteCount;
            }
            set
            {
                _satelliteCount = value;
                OnPropertyChanged(nameof(SatelliteCount));
            }
        }
        
        private double _aircraftAltitude = 0.0;
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

        private double _aircraftLongitude = 0.0;
        public double AircraftLongitude
        {
            get
            {
                return _aircraftLongitude;
            }
            set
            {
                _aircraftLongitude = value;
                OnPropertyChanged(nameof(AircraftLongitude));
            }
        }

        private double _aircraftLatitude = 0.0;
        public double AircraftLatitude
        {
            get
            {
                return _aircraftLatitude;
            }
            set
            {
                _aircraftLatitude = value;
                OnPropertyChanged(nameof(AircraftLatitude));
            }
        }

        private double _aircraftHorizontalSpeed = 0.0;
        public double AircraftHorizontalSpeed
        {
            get
            {
                return _aircraftHorizontalSpeed;
            }
            set
            {
                _aircraftHorizontalSpeed = value;
                OnPropertyChanged(nameof(AircraftHorizontalSpeed));
            }
        }

        private double _aircraftVerticalSpeed = 0.0;
        public double AircraftVerticalSpeed
        {
            get
            {
                return _aircraftVerticalSpeed;
            }
            set
            {
                _aircraftVerticalSpeed = value;
                OnPropertyChanged(nameof(AircraftVerticalSpeed));
            }
        }

        private int _swapChainRow = 1;
        public int SwapChainRow
        {
            get
            {
                return _swapChainRow;
            }
            set
            {
                _swapChainRow = value;
                OnPropertyChanged(nameof(SwapChainRow));
            }
        }

        private int _swapChainColumn = 1;
        public int SwapChainColumn
        {
            get
            {
                return _swapChainColumn;
            }
            set
            {
                _swapChainColumn = value;
                OnPropertyChanged(nameof(SwapChainColumn));
            }
        }

        private int _swapChainWidth = 1;
        public int SwapChainWidth
        {
            get
            {
                return _swapChainWidth;
            }
            set
            {
                _swapChainWidth = value;
                OnPropertyChanged(nameof(SwapChainWidth));
            }
        }

    }
}
