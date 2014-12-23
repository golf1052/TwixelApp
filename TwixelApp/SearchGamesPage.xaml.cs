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
    public sealed partial class SearchGamesPage : Page
    {
        ObservableCollection<GameGridViewBinding> gamesCollection;
        string searchQuery;
        bool clickedItem = false;

        public SearchGamesPage()
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

            gamesCollection = new ObservableCollection<GameGridViewBinding>();
            List<SearchedGame> searchedGames = new List<SearchedGame>();
            searchedGames = await AppConstants.twixel.SearchGames(searchQuery, true);

            foreach (SearchedGame searchedGame in searchedGames)
            {
                gamesCollection.Add(new GameGridViewBinding(searchedGame.ToGame()));
            }

            searchText.Text = "Searched: " + searchQuery;
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

        private void gamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = sender as GridView;
            gridView.ItemsSource = gamesCollection;
        }

        private async void gamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!clickedItem)
            {
                clickedItem = true;
                List<object> parameters = new List<object>();
                GameGridViewBinding gameItem = (GameGridViewBinding)e.ClickedItem;
                List<Game> games = new List<Game>();
                games = await AppConstants.twixel.RetrieveTopGames(100, false);
                bool foundGame = false;
                foreach (Game game in games)
                {
                    if (game.name == gameItem.game.name)
                    {
                        foundGame = true;
                        parameters.Add(game);
                        Frame.Navigate(typeof(GameStreamsPage), parameters);
                        break;
                    }
                }

                do
                {
                    if (!foundGame)
                    {
                        games = await AppConstants.twixel.RetrieveTopGames(true);
                        foreach (Game game in games)
                        {
                            if (game.name == gameItem.game.name)
                            {
                                foundGame = true;
                                parameters.Add(game);
                                Frame.Navigate(typeof(GameStreamsPage), parameters);
                                break;
                            }
                        }
                    }
                }
                while (games.Count != 0 && !foundGame);

                Frame.Navigate(typeof(GameStreamsPage), parameters);
            }
        }

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchGamesPage), parameters);
        }
    }
}
