using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
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
    public sealed partial class GamesPage : Page
    {
        ObservableCollection<GameGridViewBinding> gamesCollection;
        List<Game> games;

        public GamesPage()
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

            gamesCollection = new ObservableCollection<GameGridViewBinding>();
            games = new List<Game>();
            games = await AppConstants.twixel.RetrieveTopGames(100, false);
            if (games != null)
            {
                foreach (Game game in games)
                {
                    gamesCollection.Add(new GameGridViewBinding(game));
                }

                do
                {
                    try
                    {
                        games = await AppConstants.twixel.RetrieveTopGames(true);
                        foreach (Game game in games)
                        {
                            gamesCollection.Add(new GameGridViewBinding(game));
                        }
                    }
                    catch
                    {
                        games.Clear();
                    }
                }
                while (games.Count != 0);
            }
            else
            {
                await AppConstants.ShowErrorAndGoBack("Could not load games.\nError Code: " + AppConstants.twixel.ErrorString, Frame);
            }
        }

        private void gamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            gamesGridView.ItemsSource = gamesCollection;
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

        private void gamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<object> parameters = new List<object>();
            GameGridViewBinding gameItem = (GameGridViewBinding)e.ClickedItem;
            parameters.Add(gameItem.game);
            Frame.Navigate(typeof(GameStreamsPage), parameters);
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage));
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            // Do Nothing
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage));
        }

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchGamesPage), parameters);
        }
    }
}
