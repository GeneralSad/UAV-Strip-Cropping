using DJI.WindowsSDK.UserAccount;
using DJI.WindowsSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Xaml.Controls;
using System.Security.AccessControl;
using System.Xml;

using Newtonsoft.Json;
using Windows.UI;

namespace UAV_App.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        private UserAccountState _accountState;

        private UserAccountState accountState
        {
            get { return _accountState; }
            set
            {
                _accountState = value;
                this.accountStateTextBlock.Text = _accountState.ToString();
            }
        }

        public LoginView()
        {
            this.InitializeComponent();

            DJISDKManager.Instance.UserAccountManager.UserAccountStateChanged += UserAccountManager_UserAccountStateChanged;
            accountState = DJISDKManager.Instance.UserAccountManager.UserAccountState;
        }


        ~LoginView()
        {
            DJISDKManager.Instance.UserAccountManager.UserAccountStateChanged -= UserAccountManager_UserAccountStateChanged;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            //var loginWebView = DJISDKManager.Instance.UserAccountManager.CreateLoginView(false);

            var loginWebView2 = new WebView2();
            loginWebView2.Source = new Uri("https://account.dji.com/login?showHeader=false&showThirdPart=false&appId=windows_sdk&locale=en_US&callback_url=djilogin%3A%2F%2Fgeologin");

            if (contentGrid.Children.Count < 2)
            {
                //contentGrid.Children.Add(loginWebView);
                //Grid.SetColumn(loginWebView, 1);

                contentGrid.Children.Add(loginWebView2);
                Grid.SetColumn(loginWebView2, 2);
            }
            else
            {
                Grid.SetColumn(loginWebView2, 1);
            }
        }

        private void statusButton_Click(object sender, RoutedEventArgs e)
        {
            //WebView loginWebView = (WebView)contentGrid.Children[1];
            WebView2 loginWebView = (WebView2)contentGrid.Children[1];
        }

        private async void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await DJISDKManager.Instance.UserAccountManager.Logout();
            var messageDialog = new MessageDialog(String.Format("User account logout: {0}", res.ToString()));
            await messageDialog.ShowAsync();
        }

        private async void UserAccountManager_UserAccountStateChanged(UserAccountState state, SDKError error)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                accountState = state;
                operationResTextBlock.Text = error.ToString();
            });
        }
    }
}
