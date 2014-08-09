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
    public sealed partial class ChannelStreamScope : Page
    {
        Twixel twixel;
        List<TwitchConstants.Scope> scopes;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> param = (List<object>)e.Parameter;
            twixel = (Twixel)param[0];
            scopes = (List<TwitchConstants.Scope>)param[1];
        }

        public ChannelStreamScope()
        {
            this.InitializeComponent();
        }

        private void nextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (allowButton.IsChecked == true)
            {
                scopes.Add(TwitchConstants.Scope.ChannelStream);
            }
            List<object> param = new List<object>();
            param.Add(twixel);
            param.Add(scopes);
            Frame.Navigate(typeof(ChannelSubscriptionsScope), param);
        }
    }
}
