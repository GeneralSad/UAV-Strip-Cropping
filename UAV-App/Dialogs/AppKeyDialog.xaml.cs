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

namespace UAV_App.Dialogs
{
    public sealed partial class AppKeyDialog : ContentDialog
    {
        public static readonly DependencyProperty appKeyProperty = DependencyProperty.Register("AppKey", typeof(string), typeof(AppKeyDialog), new PropertyMetadata(default(string)));

        public AppKeyDialog()
        {
            InitializeComponent();
        }

        public string AppKey
        {
            get
            {
                return (string) GetValue(appKeyProperty);
            }
            set 
            {
                SetValue(appKeyProperty, value);
            }
        }

        private void AppKeyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void AppKeyDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
