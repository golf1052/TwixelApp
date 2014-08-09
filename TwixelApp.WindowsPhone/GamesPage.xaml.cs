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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamesPage : Page
    {
        Twixel twixel;
        ObservableCollection<GameGridViewBinding> gamesCollection;
        List<Game> games;

        public GamesPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            twixel = (Twixel)e.Parameter;

            if (twixel.users.Count > 0)
            {
                if (twixel.users[0].authorized)
                {
                    userButton.Content = twixel.users[0].displayName;
                }
                else
                {
                    userButton.Content = "Not Logged In";
                    userButton.IsEnabled = false;
                }
            }
            else
            {
                userButton.Content = "Not Logged In";
                userButton.IsEnabled = false;
            }

            gamesCollection = new ObservableCollection<GameGridViewBinding>();
            games = new List<Game>();
            games = await twixel.RetrieveTopGames(100, false);
            foreach (Game game in games)
            {
                gamesCollection.Add(new GameGridViewBinding(game));
            }

            do
            {
                games = await twixel.RetrieveTopGames(true);
                foreach (Game game in games)
                {
                    gamesCollection.Add(new GameGridViewBinding(game));
                }
            }
            while (games.Count != 0);
        }

        private void gamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            gamesGridView.ItemsSource = gamesCollection;
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), twixel);
        }

        private void gamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            GameGridViewBinding gameItem = (GameGridViewBinding)e.ClickedItem;
            parameters.Add(gameItem.game);
            Frame.Navigate(typeof(GameStreamsPage), parameters);
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage), twixel);
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            // Do Nothing
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage), twixel);
        }
    }
}
