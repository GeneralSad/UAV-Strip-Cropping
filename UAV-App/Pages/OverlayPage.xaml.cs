using DJI.WindowsSDK;
using DJIVideoParser;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using UAV_App.Drone_Patrol;
using UAV_App.ViewModels;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UAV_App.Pages
{
    public sealed partial class OverlayPage : Page
    {

        //Singleton to acces public methods
        private static OverlayPage _singleton;
        public static OverlayPage Instance
        {
            get
            {
                return _singleton;
            }
        }

        public bool IsVideoFeedActive = false;

        private Parser videoParser;

        //Connect event handlers
        public OverlayPage()
        {
            this.InitializeComponent();

            if (Instance == null)
            {
                _singleton = this;
            }

            OverlayViewModel.Instance.PropertyChanged += ViewModel_PropertyChanged;
        }

        //Event handler for when properties change in viewmodel
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        //Event handler for when overlay opens
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = OverlayViewModel.Instance;
            base.OnNavigatedFrom(e);
        }

        //Event handler for when overlay closes
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        //Start live video overlay for overlay initialization
        public async void StartVideoFeed()
        {
            InitializeVideoFeedModule();
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraWorkModeAsync(new CameraWorkModeMsg { value = CameraWorkMode.SHOOT_PHOTO });
            IsVideoFeedActive = true;
        }

        //Stop video overlay when needed
        public void StopVideoFeed()
        {
            UninitializeVideoFeedModule();
            IsVideoFeedActive = false;
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

        //Remove event handlers from events to stop video feed
        private void UninitializeVideoFeedModule()
        {
            if (DJISDKManager.Instance.SDKRegistrationResultCode == SDKError.NO_ERROR)
            {
                videoParser.SetSurfaceAndVideoCallback(0, 0, null, null);
                DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated -= OnVideoPush;
            }
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

        //Handle raw data
        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        //Decode data. This function receives a byte array with image data in RGBA format.
        void ReceiveDecodedData(byte[] data, int width, int height)
        {
        }

    }
}
