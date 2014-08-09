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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchStreamsPage : Page
    {
        Twixel twixel;
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        string searchQuery;

        public SearchStreamsPage()
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
                    userButton.Content = "Not Logged In";
                    userButton.IsEnabled = false;
                }
            }
            else
            {
                userButton.Content = "Not Logged In";
                userButton.IsEnabled = false;
            }

            streamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            List<TwixelAPI.Stream> searchedStreams = new List<TwixelAPI.Stream>();
            searchedStreams = await twixel.SearchStreams(searchQuery, 100);
            foreach (TwixelAPI.Stream stream in searchedStreams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }
            searchText.Text = "Searched: " + searchQuery;
            gameChannels.Text = searchedStreams.Count.ToString();
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
            Frame.Navigate(typeof(UserPage), twixel);
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
            parameters.Add(twixel);
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            parameters.Add(searchBox.QueryText);
            Frame.Navigate(typeof(SearchStreamsPage), parameters);
        }
    }
}
