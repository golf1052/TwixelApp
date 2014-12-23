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
    public sealed partial class ChannelReadScope : Page
    {
        List<TwitchConstants.Scope> scopes;

        public ChannelReadScope()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> param = (List<object>)e.Parameter;
            scopes = (List<TwitchConstants.Scope>)param[0];
        }

        private void nextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (allowButton.IsChecked == true)
            {
                scopes.Add(TwitchConstants.Scope.ChannelRead);
            }
            List<object> param = new List<object>();
            param.Add(scopes);
            Frame.Navigate(typeof(ChannelEditorScope), param);
        }
    }
}
