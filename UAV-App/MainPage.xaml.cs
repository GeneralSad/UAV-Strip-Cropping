using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UAV_App.Database;
using UAV_App.Dialogs;
using UAV_App.Pages;
using UAV_App.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UAV_App
{
    public sealed partial class MainPage : Page
    {

        private List<KeyValuePair<String, Type>> menuItems = new List<KeyValuePair<String, Type>>()
        {
             new KeyValuePair<string, Type>("SDK Registration", typeof(Pages.SDKPage)),
             new KeyValuePair<string, Type>("Patrol Control", typeof(Pages.RoutePage)),
             //new KeyValuePair<string, Type>("Information rapport", typeof(Pages.InformationRapportPage)),
        };

        public MainPage()
        {
            this.InitializeComponent();

            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            ApplicationSettings applicationSettings = new ApplicationSettings();
            applicationSettings.LoadSettingsAsync();

            var item = menuItems[0];
            NavView.MenuItems.Add(item.Key);

            ContentFrame.Navigate(typeof(Pages.OverlayPage));
            setContentFrameContent(typeof(Pages.SDKPage));
        }

        private void setContentFrameContent(Type contentType)
        {
            var overlayPage = ContentFrame.Content as Page;
            var grid = overlayPage.Content as Grid;
            var contentFrame = grid.Children[0] as Frame;
            if (contentFrame.SourcePageType != contentType)
            {
                Debug.WriteLine("ContentType navigating to: " + contentType);
                contentFrame.Navigate(contentType);
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            String invokedName = args.InvokedItem as String;
            foreach (var item in menuItems)
            {
                if (invokedName == item.Key)
                {
                    setContentFrameContent(item.Value);
                    return;
                }
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {

                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                string storedTempAppKey = localSettings.Values["TempAppKey"] as string;

                if (storedTempAppKey != null)
                {
                    // Save a setting locally on the device
                    localSettings.Values["AppKey"] = storedTempAppKey;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    OverlayViewModel.Instance.StartOverlay();
                    if (OverlayPage.Instance.IsVideoFeedActive == false)
                    {
                        OverlayPage.Instance.StartVideoFeed();
                    }

                    NavView.MenuItems.RemoveAt(0);

                    for (int i = 1; i < menuItems.Count; ++i)
                    {
                        var item = menuItems[i];
                        NavView.MenuItems.Add(item.Key);
                    }

                    setContentFrameContent(typeof(Pages.RoutePage));
                });

            }
        }
    }
}
