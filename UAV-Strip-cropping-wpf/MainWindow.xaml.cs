﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DJI.WindowsSDK;

namespace UAV_Strip_cropping_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            var instance = DJISDKManager.Instance;
            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            // DJISDKManager.Instance.RegisterApp("20c067ae1fe8582f566f5e77");
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                //    System.Diagnostics.Debug.WriteLine("Register app successfully.");

                //    //The product connection state will be updated when it changes here.
                //    DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                //    {
                //        await Dispatcher.InvokeAsync(System.Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                //        {
                //            if (value != null && value?.value != ProductType.UNRECOGNIZED)
                //            {
                //                System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                //                //You can load/display your pages according to the aircraft connection state here.
                //            }
                //            else
                //            {
                //                System.Diagnostics.Debug.WriteLine("The Aircraft is disconnected now.");
                //                //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                //            }
                //        });
                //    };

                //    //If you want to get the latest product connection state manually, you can use the following code
                //    var productType = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
                //    if (productType != null && productType?.value != ProductType.UNRECOGNIZED)
                //    {
                //        System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                //        //You can load/display your pages according to the aircraft connection state here.
                //    }
                //}
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine("Register SDK failed, the error is: ");
                //    System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            }
        }

        private void sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = sidebar.SelectedItem as NavButton;

            navframe.Navigate(selected?.Navlink);

        }
    }
}
  