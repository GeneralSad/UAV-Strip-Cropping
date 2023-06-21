using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UAV_App.Database;
using UAV_App.Dialogs;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UAV_App.Pages
{
    public sealed partial class SDKPage : Page
    {
        //Page for when no app key has been set and the user clicked on cancel
        //This way the user won't have to start up the application again to see the popup
        public SDKPage()
        {
            this.InitializeComponent();



        }

        //Load the settings when button has been clicked
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplicationSettings applicationSettings = new ApplicationSettings();
            applicationSettings.LoadSettingsAsync();
        }
    }
}
