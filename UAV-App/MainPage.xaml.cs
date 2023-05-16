using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UAV_App
{
    public sealed partial class MainPage : Page
    {

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
            setContentFrameContent(typeof(DJISDKInitializing.ActivatingPage));
            
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
            foreach (var module in navigationModules)
            {
                foreach (var item in module.items)
                {
                    if (invokedName == item.Key)
                    {
                        setContentFrameContent(item.Value);
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
