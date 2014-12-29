using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            gamesCollection = new ObservableCollection<GameGridViewBinding>();
            games = new List<Game>();
            await AppConstants.SetText("All Games");
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

        }

        private void gamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
