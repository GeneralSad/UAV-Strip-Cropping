using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UAV_App
{
    public sealed partial class MainPage : Page
    {
        public bool isActivated = false;
        private struct SDKModuleSampleItems
        {
            public String header;
            public List<KeyValuePair<String, Type>> items;
        }

        private List<SDKModuleSampleItems> navigationModules = new List<SDKModuleSampleItems>
        {
            new SDKModuleSampleItems() {
                header = "Activation", items = new List<KeyValuePair<String, Type>>()
                {
                    new KeyValuePair<string, Type>("Activating DJIWindowsSDK", typeof(DJISDKInitializing.ActivatingPage)),
                },
            },
            new SDKModuleSampleItems() {
                header = "Account", items = new List<KeyValuePair<String, Type>>()
                {
                    new KeyValuePair<string, Type>("Account Management", typeof(Pages.LoginView)),
                },
            },
            new SDKModuleSampleItems() {
                header = "Information", items = new List<KeyValuePair<String, Type>>()
                {
                    new KeyValuePair<string, Type>("Information rapport", typeof(Pages.InformationRapportPage)),
                },
            },
            new SDKModuleSampleItems() {
                header = "Camera", items = new List<KeyValuePair<String, Type>>()
                {
                    new KeyValuePair<string, Type>("Video stream", typeof(Pages.FPVPage)),
                },
            },
        };

        public MainPage()
        {
            this.InitializeComponent();
            var module = navigationModules[0];
            NavView.MenuItems.Add(new NavigationViewItemHeader() { Content = module.header });
            foreach (var item in module.items)
            {
                NavView.MenuItems.Add(item.Key);
            }
            ContentFrame.Navigate(typeof(Pages.OverlayPage));
            var a = ContentFrame.Content as Page;
            var grid = a.Content as Grid;
            var content = grid.Children[0] as Frame;
            content.Navigate(typeof(DJISDKInitializing.ActivatingPage));
        }

        public bool getActivated()
        {
            return isActivated;
        }
        public void setActivated(bool isActive)
        {
            isActivated = isActive;
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            String invokedName = args.InvokedItem as String;
            foreach (var module in navigationModules)
            {
                foreach (var item in module.items)
                {
                    if (invokedName == item.Key)
                    {
                        if (ContentFrame.SourcePageType != item.Value)
                        {
                            var a = ContentFrame.Content as Page;
                            var grid = a.Content as Grid;
                            var content = grid.Children[0] as Frame;
                            content.Navigate(item.Value);
/*                            content.Navigate(item.Value, isActivated);
*/                        }
                        return;
                    }
                }
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    for (int i = 1; i < navigationModules.Count; ++i)
                    {
                        var module = navigationModules[i];
                        NavView.MenuItems.Add(new NavigationViewItemHeader() { Content = module.header });
                        foreach (var item in module.items)
                        {
                            NavView.MenuItems.Add(item.Key);
                        }
                    }
                });
            }
        }
    }
}
