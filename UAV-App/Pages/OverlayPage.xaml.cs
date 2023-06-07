using DJI.WindowsSDK;
using DJIVideoParser;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using UAV_App.Drone_Patrol;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UAV_App.Pages
{
    public sealed partial class OverlayPage : Page
    {

        public static OverlayPage Current;

        public bool IsVideoFeedActive;

        private Parser videoParser;

        public OverlayPage()
        {
            this.InitializeComponent();

            if (Current == null)
            {
                Current = this;
            }
            swapChainPanel.Tapped += OnFeedTapped;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void InitializeVideoFeedModule()
        {
            //Must run in UI thread
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (videoParser == null)
                {
                    videoParser = new Parser();
                    videoParser.Initialize(delegate (byte[] data)
                    {
                        //Note: This function must be called because we need DJI Windows SDK to help us to parse frame data.
                        return DJISDKManager.Instance.VideoFeeder.ParseAssitantDecodingInfo(0, data);
                    });

                    //Set the swapChainPanel to display and set the decoded data callback.
                    videoParser.SetSurfaceAndVideoCallback(0, 0, swapChainPanel, ReceiveDecodedData);
                    DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated += OnVideoPush;
                }

                //get the camera type and observe the CameraTypeChanged event.
                DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).CameraTypeChanged += OnCameraTypeChanged;
                var type = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).GetCameraTypeAsync();
                OnCameraTypeChanged(this, type.value);
            });
        }

        private void UninitializeVideoFeedModule()
        {
            if (DJISDKManager.Instance.SDKRegistrationResultCode == SDKError.NO_ERROR)
            {
                videoParser.SetSurfaceAndVideoCallback(0, 0, null, null);
                DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated -= OnVideoPush;
            }
        }

        //raw data
        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        //Decode data. Do nothing here. This function would return a bytes array with image data in RGBA format.
        void ReceiveDecodedData(byte[] data, int width, int height)
        {
        }

        //We need to set the camera type of the aircraft to the DJIVideoParser. After setting camera type, DJIVideoParser would correct the distortion of the video automatically.
        private void OnCameraTypeChanged(object sender, CameraTypeMsg? value)
        {
            if (value != null)
            {
                switch (value.Value.value)
                {
                    case CameraType.MAVIC_2_ZOOM:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Zoom);
                        break;
                    case CameraType.MAVIC_2_PRO:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Pro);
                        break;
                    default:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Others);
                        break;
                }

            }
        }

        public async void StartVideoFeed()
        {
            InitializeVideoFeedModule();
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraWorkModeAsync(new CameraWorkModeMsg { value = CameraWorkMode.SHOOT_PHOTO });
            IsVideoFeedActive = true;
        }

        public void StopVideoFeed()
        {
            UninitializeVideoFeedModule();
            IsVideoFeedActive = false;
        }

        public void ToggleFullscreen()
        {
            if ((int) swapChainPanelBox.GetValue(Grid.RowProperty) == 1)
            {
                swapChainPanelBox.SetValue(Grid.RowProperty, 0);
                swapChainPanelBox.SetValue(Grid.ColumnProperty, 0);
            }
            else
            {
                swapChainPanelBox.SetValue(Grid.RowProperty, 1);
                swapChainPanelBox.SetValue(Grid.ColumnProperty, 1);
            }
        }

        public SwapChainPanel GetFeed()
        {
            return swapChainPanel;
        }

        private void OnFeedTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        private async void EmergencyButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Debug.WriteLine("Emergency");
        }

        public async void BatteryPercentageChanged(object sender, IntMsg? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    BatteryLevelTextBlock.Text = value.Value.value + "%";
                }
            });
        }

        public async void SatelliteCountChanged(object sender, IntMsg? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    SatelliteCountTextBlock.Text = value.Value.value.ToString();
                }
            });
        }

        public async void AircraftAltitudeChanged(object sender, DoubleMsg? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftAltitudeTextBlock.Text = value.Value.value + "m";
                }
            });
        }

        public async void AircraftLocationChanged(object sender, LocationCoordinate2D? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    AircraftLongitudeTextBlock.Text =  "Lon: " + Math.Round(value.Value.longitude, 6);
                    AircraftLatitudeTextBlock.Text = "Lat: " + Math.Round(value.Value.latitude, 6);
                }
            });
        }

        public async void AircraftHomeLocationChanged(object sender, BoolMsg? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
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
                    SetHomeButton.Background = new SolidColorBrush(color);
                }
            });
        }

        public async void AircraftVelocityChanged(object sender, Velocity3D? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (value.HasValue)
                {
                    double horizontalSpeed = Math.Abs(value.Value.x) + Math.Abs(value.Value.y);
                    double verticalSpeed = Math.Abs(value.Value.z);  
                    AircraftHorizontalSpeedTextBlock.Text = "H.S: " + Math.Round(horizontalSpeed, 1);
                    AircraftVerticalSpeedTextBlock.Text = "V.S: " + Math.Round(verticalSpeed, 1);
                }
            });
        }

        private async void SetHomeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var value = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync();
            LocationCoordinate2D location = value.value.Value;
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetHomeLocationAsync(location);
        }

    }
}
