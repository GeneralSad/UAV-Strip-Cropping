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
using Windows.UI.Xaml.Media;

namespace UAV_App.ViewModels
{
    internal class OverlayViewModel : ViewModelBase
    {
        private static readonly OverlayViewModel _singleton = new OverlayViewModel();
        public static OverlayViewModel Instance
        {
            get
            {
                return _singleton;
            }
        }

        private OverlayViewModel()
        {
        }

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

        public ICommand _emergencyStop;
        public ICommand EmergencyStop
        {
            get
            {
                if (_emergencyStop == null)
                {
                    _emergencyStop = new RelayCommand(async delegate ()
                    {
                        Debug.WriteLine("Emergency");
                    }, delegate () { return true; });
                }
                return _emergencyStop;
            }
        }

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
        
        private SolidColorBrush _setHomeColor = new SolidColorBrush(Colors.Red);
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
    }
}
