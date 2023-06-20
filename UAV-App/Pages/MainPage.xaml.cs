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

        //List of the menu items, with the name to be shown and page it should link to
        private List<KeyValuePair<string, Type>> menuItems = new List<KeyValuePair<string, Type>>()
        {
             new KeyValuePair<string, Type>("SDK Registration", typeof(Pages.SDKPage)),
             new KeyValuePair<string, Type>("Patrol Control", typeof(Pages.RoutePage)),
             new KeyValuePair<string, Type>("Information rapport", typeof(Pages.InformationRapportPage)),
        };

        public MainPage()
        {
            this.InitializeComponent();

            //Add event handler
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            //Load in application settings
            ApplicationSettings applicationSettings = new ApplicationSettings();
            _ = applicationSettings.LoadSettingsAsync();

            //Add menu item to the menu
            var item = menuItems[0];
            NavView.MenuItems.Add(item.Key);

            //Set content of the page to the overlay and the SDK page on startup
            ContentFrame.Navigate(typeof(Pages.OverlayPage));
            SetContentFrameContent(typeof(Pages.SDKPage));
        }

        private void SetContentFrameContent(Type contentType)
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
                    SetContentFrameContent(item.Value);
                    return;
                }
            }
        }

        //When navigation vies has loaded, not used. But has to be there to use navigation view
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        //Gets called when the register method is used. Returns an error with the reason of failed registration or not
        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {

                //Load the localsettings
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                //Get temporary app key
                string storedTempAppKey = localSettings.Values["TempAppKey"] as string;

                //SDK has been successfully registered, so temporary app key works and can be saved permanently
                if (storedTempAppKey != null)
                {
                    // Save a setting locally on the device
                    localSettings.Values["AppKey"] = storedTempAppKey;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //Start the overly and if the videofeed is not active yet, the videofeed too
                    OverlayViewModel.Instance.StartOverlay();
                    if (OverlayPage.Instance.IsVideoFeedActive == false)
                    {
                        OverlayPage.Instance.StartVideoFeed();
                    }

                    //Remove the SDK page from the menu, because SDK has been registered
                    NavView.MenuItems.RemoveAt(0);

                    //Add All remaining pages to the menu
                    for (int i = 1; i < menuItems.Count; ++i)
                    {
                        var item = menuItems[i];
                        NavView.MenuItems.Add(item.Key);
                    }

                    //Set frame to the route page
                    SetContentFrameContent(typeof(Pages.RoutePage));
                });

            }
        }
    }
}
