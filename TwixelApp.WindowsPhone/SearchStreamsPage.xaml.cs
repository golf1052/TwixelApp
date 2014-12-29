using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchStreamsPage : Page
    {
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        string searchQuery;
        SearchFlyout searchFlyout;

        public SearchStreamsPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = (List<object>)e.Parameter;
            searchQuery = (string)parameters[0];
            await AppConstants.SetText("Searching for " + searchQuery);

            if (AppConstants.ActiveUser != null)
            {
                if (AppConstants.ActiveUser.authorized)
                {
                    userButton.Content = AppConstants.ActiveUser.displayName;
                }
                else
                {
                    userButton.Content = "Log In";
                }
            }
            else
            {
                userButton.Content = "Log In";
            }

            StatusBar statusBar = AppConstants.GetStatusBar();
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();
            streamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            List<TwixelAPI.Stream> searchedStreams = new List<TwixelAPI.Stream>();
            searchedStreams = await AppConstants.twixel.SearchStreams(searchQuery, 100);
            foreach (TwixelAPI.Stream stream in searchedStreams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }
            await AppConstants.SetText(searchedStreams.Count.ToString() + " streams. Searched: " + searchQuery);
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppConstants.ActiveUser == null || !AppConstants.ActiveUser.authorized)
            {
                List<TwitchConstants.Scope> scopes = new List<TwitchConstants.Scope>();
                List<object> param = new List<object>();
                param.Add(scopes);
                Frame.Navigate(typeof(UserReadScope), param);
            }
            else
            {
                //Frame.Navigate(typeof(UserPage));
            }
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void searchButton_Loaded(object sender, RoutedEventArgs e)
        {
            searchFlyout = new SearchFlyout(searchBox, startSearchButton, searchComboBox, Frame);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void streamsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            streamsGridView.ItemsSource = streamsCollection;
        }

        private async void streamsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;

            List<object> parameters = new List<object>();
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }
    }
}
