using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
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
    public sealed partial class SearchGamesPage : Page
    {
        ObservableCollection<GameGridViewBinding> gamesCollection;
        string searchQuery;
        bool clickedItem = false;
        SearchFlyout searchFlyout;

        public SearchGamesPage()
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

            gamesCollection = new ObservableCollection<GameGridViewBinding>();
            List<SearchedGame> searchedGames = new List<SearchedGame>();
            StatusBar statusBar = AppConstants.GetStatusBar();
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();
            searchedGames = await AppConstants.twixel.SearchGames(searchQuery, true);
            foreach (SearchedGame searchedGame in searchedGames)
            {
                gamesCollection.Add(new GameGridViewBinding(searchedGame.ToGame()));
            }
            await AppConstants.SetText("Searched: " + searchQuery);
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

        private void gamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            gamesGridView.ItemsSource = gamesCollection;
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

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
