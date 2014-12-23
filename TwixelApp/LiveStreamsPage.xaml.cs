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
    public sealed partial class LiveStreamsPage : Page
    {
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        List<TwixelAPI.Stream> streams;
        bool tappedStream = false;

        public LiveStreamsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
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
            streams = new List<TwixelAPI.Stream>();
            streams = await AppConstants.twixel.RetrieveStreams("", new List<string>(), 100, false, false);
            foreach (TwixelAPI.Stream stream in streams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }

            do
            {
                streams = await AppConstants.twixel.RetrieveStreams(true);
                foreach (TwixelAPI.Stream stream in streams)
                {
                    streamsCollection.Add(new GameStreamsGridViewBinding(stream));
                }
            }
            while (streams.Count != 0);
        }

        private void streamsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            streamsGridView.ItemsSource = streamsCollection;
        }

        private async void streamsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (tappedStream == false)
            {
                tappedStream = true;
                GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;
                List<object> parameters = new List<object>();
                parameters.Add(streamItem.stream);
                Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
                parameters.Add(qualities);
                Frame.Navigate(typeof(StreamPage), parameters);
            }
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage));
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

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage));
        }

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchStreamsPage), parameters);
        }
    }
}
