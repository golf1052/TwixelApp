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
    public sealed partial class LiveStreamsPage : Page
    {
        Twixel twixel;
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        List<TwixelAPI.Stream> streams;

        public LiveStreamsPage()
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
                    //userButton.Content = twixel.users[0].displayName;
                }
                else
                {
                    //userButton.Content = "Not Logged In";
                    //userButton.IsEnabled = false;
                }
            }
            else
            {
                //userButton.Content = "Not Logged In";
                //userButton.IsEnabled = false;
            }

            streamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            streams = new List<TwixelAPI.Stream>();
            streams = await twixel.RetrieveStreams("", new List<string>(), 100, false, false);
            foreach (TwixelAPI.Stream stream in streams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }

            do
            {
                streams = await twixel.RetrieveStreams(true);
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
            GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage), twixel);
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), twixel);
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage), twixel);
        }
    }
}
