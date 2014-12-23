using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchStreamsPage : Page
    {
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        string searchQuery;

        public SearchStreamsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = (List<object>)e.Parameter;
            searchQuery = (string)parameters[0];
            searchText.Text = "Search Query: " + searchQuery;

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

            streamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            List<TwixelAPI.Stream> searchedStreams = new List<TwixelAPI.Stream>();
            searchedStreams = await AppConstants.twixel.SearchStreams(searchQuery, 100);
            foreach (TwixelAPI.Stream stream in searchedStreams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }
            searchText.Text = "Searched: " + searchQuery;
            gameChannels.Text = searchedStreams.Count.ToString();
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage));
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage));
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage));
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
                Frame.Navigate(typeof(UserPage));
            }
        }

        private void streamsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = sender as GridView;
            gridView.ItemsSource = streamsCollection;
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

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchStreamsPage), parameters);
        }
    }
}
