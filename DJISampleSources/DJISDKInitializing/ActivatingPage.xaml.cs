using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using UAV_App.Pages;

namespace UAV_App.DJISDKInitializing
{
    public sealed partial class ActivatingPage : Page
    {
        public ActivatingPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                activateStateTextBlock.Text = state == SDKRegistrationState.Succeeded ? "Activated." : "Not Activated.";
                activationInformation.Text = resultCode == SDKError.NO_ERROR ? "Register success" : resultCode.ToString();
                if (resultCode == SDKError.NO_ERROR)
                {
                    if (OverlayPage.Current?.IsVideoFeedActive == false)
                    {
                        OverlayPage.Current?.StartVideoFeed();
                    }

                    DJISDKManager.Instance.ComponentManager.GetBatteryHandler(0, 0).ChargeRemainingInPercentChanged += OverlayPage.Current.BatteryPercentageChanged;
                    DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SatelliteCountChanged += OverlayPage.Current.SatelliteCountChanged;
                    DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AltitudeChanged += OverlayPage.Current.AircraftAltitudeChanged;
                    DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += OverlayPage.Current.AircraftLocationChanged;
                    DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).IsHomeLocationSetChanged += OverlayPage.Current.AircraftHomeLocationChanged;

                }
            });
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            DJISDKManager.Instance.RegisterApp(activatingCodeTextBox.Text);
            activationInformation.Text = "Registering...";
        }

    }
}
