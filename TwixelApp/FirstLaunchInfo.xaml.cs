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
using TwixelAPI.Constants;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstLaunchInfo : Page
    {
        Twixel twixel;

        public FirstLaunchInfo()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            welcomeBlock.FontSize = 36;
            loginBlock.FontSize = 24;
            continueBlock.FontSize = 24;
#endif
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            twixel = (Twixel)e.Parameter;
        }

        private void loginBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            List<TwitchConstants.Scope> scopes = new List<TwitchConstants.Scope>();
            List<object> param = new List<object>();
            param.Add(twixel);
            param.Add(scopes);
            Frame.Navigate(typeof(UserReadScope), param);
        }

        private void continueBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }
    }
}
