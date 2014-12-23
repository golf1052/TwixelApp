using System.Collections.Generic;
using TwixelAPI.Constants;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstLaunchInfo : Page
    {
        public FirstLaunchInfo()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void loginBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            List<TwitchConstants.Scope> scopes = new List<TwitchConstants.Scope>();
            List<object> param = new List<object>();
            param.Add(scopes);
            Frame.Navigate(typeof(UserReadScope), param);
        }

        private void continueBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Frame.Navigate(typeof(HomePage));
        }
    }
}
