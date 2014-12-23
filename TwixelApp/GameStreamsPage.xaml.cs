using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameStreamsPage : Page
    {
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        bool loadedStream = false;

        public GameStreamsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = (List<object>)e.Parameter;
            Game game = (Game)parameters[0];
            BitmapImage gameImage = new BitmapImage(game.logoMedium.url);
            gameLogo.Source = gameImage;
            gameName.Text = game.name;
            gameChannels.Text = game.channels.ToString();
            gameViewers.Text = game.viewers.ToString();

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
            List<TwixelAPI.Stream> streams = new List<TwixelAPI.Stream>();
            streams = await AppConstants.twixel.RetrieveStreams(game.name, new List<string>(), 100, false, false);
            foreach (TwixelAPI.Stream stream in streams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }

            do
            {
                // Fix implemented so that streams
                // stop loading after you navigate away from the page
                if (loadedStream)
                {
                    break;
                }

                streams = await AppConstants.twixel.RetrieveStreams(true);
                foreach (TwixelAPI.Stream stream in streams)
                {
                    if (loadedStream)
                    {
                        break;
                    }

                    streamsCollection.Add(new GameStreamsGridViewBinding(stream));
                }

                if (loadedStream)
                {
                    break;
                }
            }
            while (streams.Count != 0);
        }

        private void gameStreamsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            gridView.ItemsSource = streamsCollection;
        }

        private async void gameStreamsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            loadedStream = true;
            GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;

            List<object> parameters = new List<object>();
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
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

        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchStreamsPage), parameters);
        }
    }
}
