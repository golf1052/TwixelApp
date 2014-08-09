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
using TwixelAPI;
using TwixelApp.Constants;
using TwixelAPI.Constants;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace TwixelApp
{
    public sealed partial class AccountsFlyout : SettingsFlyout
    {
        Twixel twixel;
        Frame frame;

        public AccountsFlyout(Twixel twixel, Frame frame)
        {
            this.twixel = twixel;
            this.frame = frame;
            this.InitializeComponent();
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            //foreach (User user in twixel.users)
            //{
            //    if (user.authorized)
            //    {
            //        accountsListView.Items.Add(user.name);
            //        if (user == AppConstants.ActiveUser)
            //        {
            //            accountsListView.SelectedIndex = accountsListView.Items.Count - 1;
            //        }
            //    }
            //}
        }

        private void accountsListView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void loginLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            List<TwitchConstants.Scope> scopes = new List<TwitchConstants.Scope>();
            List<object> param = new List<object>();
            param.Add(twixel);
            param.Add(scopes);
            frame.Navigate(typeof(UserReadScope), param);
        }
    }
}
