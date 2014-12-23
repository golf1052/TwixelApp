using System.Collections.Generic;
using TwixelAPI.Constants;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace TwixelApp
{
    public sealed partial class AccountsFlyout : SettingsFlyout
    {
        Frame frame;

        public AccountsFlyout(Frame frame)
        {
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
            param.Add(scopes);
            frame.Navigate(typeof(UserReadScope), param);
        }
    }
}
