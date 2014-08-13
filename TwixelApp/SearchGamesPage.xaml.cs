using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TwixelApp.Constants;
using TwixelAPI.Constants;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchGamesPage : Page
    {
        Twixel twixel;
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
            twixel = (Twixel)parameters[0];
            searchQuery = (string)parameters[1];
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
            searchedGames = await twixel.SearchGames(searchQuery, true);

            foreach (SearchedGame searchedGame in searchedGames)
            {
                gamesCollection.Add(new GameGridViewBinding(searchedGame.ToGame()));
            }

            searchText.Text = "Searched: " + searchQuery;
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage), twixel);
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage), twixel);
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage), twixel);
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppConstants.ActiveUser == null || !AppConstants.ActiveUser.authorized)
            {
                List<TwitchConstants.Scope> scopes = new List<TwitchConstants.Scope>();
                List<object> param = new List<object>();
                param.Add(twixel);
                param.Add(scopes);
                Frame.Navigate(typeof(UserReadScope), param);
            }
            else
            {
                Frame.Navigate(typeof(UserPage), twixel);
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
                parameters.Add(twixel);
                GameGridViewBinding gameItem = (GameGridViewBinding)e.ClickedItem;
                List<Game> games = new List<Game>();
                games = await twixel.RetrieveTopGames(100, false);
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
                        games = await twixel.RetrieveTopGames(true);
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
            parameters.Add(twixel);
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchGamesPage), parameters);
        }
    }
}
