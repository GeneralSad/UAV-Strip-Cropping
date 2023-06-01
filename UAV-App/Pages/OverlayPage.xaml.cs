using DJI.WindowsSDK;
using DJIVideoParser;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using UAV_App.Drone_Patrol;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
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

        private void EmergencyButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Debug.WriteLine("Emergency");
        }

        private async void TakeOffButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var shouldTakeOff = false;
            var shouldTakeOffMessageDialog = new MessageDialog("Are you sure you wish to take off?");

            shouldTakeOffMessageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler((IUICommand _) => shouldTakeOff = true)));
            shouldTakeOffMessageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler((IUICommand _) => shouldTakeOff = false)));
            await shouldTakeOffMessageDialog.ShowAsync();

            if (shouldTakeOff)
            {
                var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
                Debug.WriteLine("Start send takeoff command: {0}", res.ToString());
                Debug.WriteLine("Take off");
            }

        }
        private async void LandButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var shouldLand = false;
            var shouldLandMessageDialog = new MessageDialog("Are you sure you wish to land?");

            shouldLandMessageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler((IUICommand _) => shouldLand = true)));
            shouldLandMessageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler((IUICommand _) => shouldLand = false)));
            await shouldLandMessageDialog.ShowAsync();

            if (shouldLand)
            {
                var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync();
                Debug.WriteLine("Start send landing command: {0}", res.ToString());
                Debug.WriteLine("Land");
            }
        }

    }
}
