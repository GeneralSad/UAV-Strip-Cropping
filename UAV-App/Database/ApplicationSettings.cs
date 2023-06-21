using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAV_App.Dialogs;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace UAV_App.Database
{
    internal class ApplicationSettings
    {
        public async Task LoadSettingsAsync()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // load a setting that is local to the device
            string storedAppKey = localSettings.Values["AppKey"] as string;

            if (storedAppKey != null)
            {
                //Register SDK
                DJISDKManager.Instance.RegisterApp(storedAppKey);
            }
            else
            {
                //Ask for the app key, and try to registet the app
                var appKeyDialog = new AppKeyDialog();
                var result = await appKeyDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var appKey = appKeyDialog.AppKey;

                    // Save a setting locally on the device
                    localSettings.Values["TempAppKey"] = appKey;

                    DJISDKManager.Instance.RegisterApp(appKey);
                }
            }
        }
    }
}
