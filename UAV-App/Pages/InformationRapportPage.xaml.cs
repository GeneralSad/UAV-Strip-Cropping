using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UAV_App.ViewModels;
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

    public sealed partial class InformationRapportPage : Page
    {
        public InformationRapportPage()
        {
            this.InitializeComponent();
        }

        //Set datacontext when the page opens, for MVVM
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = InformationRapportViewModel.Instance;
            base.OnNavigatedTo(e);
        }
    }
}
