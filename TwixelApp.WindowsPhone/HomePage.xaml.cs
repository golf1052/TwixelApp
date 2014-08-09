using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
using System.Diagnostics;
using Windows.System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using SM.Media;
using SM.Media.Playlists;
using SM.Media.Segments;
using SM.Media.Web;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        Twixel twixel;
        ObservableCollection<GameGridViewBinding> topGamesCollection;

        List<Game> justFetchedTopGames = new List<Game>();

        List<FeaturedStream> featuredStreams = new List<FeaturedStream>();
        AppBarButton backStreamButton;
        AppBarButton forwardStreamButton;
        AppBarButton pausePlayButton;
        TextBlock featuredGameTextBlock;
        Grid mainGrid;
        MediaElement featuredStreamPlayer;
        TextBlock featuredDescritpionTextBlock;
        int selectedStreamIndex;
        Dictionary<AppConstants.Quality, Uri> qualities;

        bool videoPlaying = false;

        StreamerObject streamerObject;

        public HomePage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
        }

        void Current_Resuming(object sender, object e)
        {
            if (videoPlaying)
            {
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
            }
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            streamerObject.Stop();
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

            topGamesCollection = new ObservableCollection<GameGridViewBinding>();
            justFetchedTopGames = new List<Game>();
            justFetchedTopGames = await twixel.RetrieveTopGames(10, false);
            foreach (Game game in justFetchedTopGames)
            {
                topGamesCollection.Add(new GameGridViewBinding(game));
            }

            featuredStreams = await twixel.RetrieveFeaturedStreams(5, false);
            if (featuredStreams.Count > 0)
            {
                selectedStreamIndex = 0;
                backStreamButton.IsEnabled = false;
                featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
                featuredDescritpionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
                qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
                videoPlaying = true;
            }
        }

        public static string FixDescription(string description)
        {
            string[] split = description.Split('\n');
            return Regex.Replace(split[0], "<.*?>", string.Empty);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), twixel);
        }

        private void topGamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            gridView.ItemsSource = topGamesCollection;
        }

        private void featuredGameTitle_Loaded(object sender, RoutedEventArgs e)
        {
            featuredGameTextBlock = (TextBlock)sender;
        }

        private void featuredGameText_Loaded(object sender, RoutedEventArgs e)
        {
            featuredDescritpionTextBlock = (TextBlock)sender;
        }

        private void topGamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            GameGridViewBinding gameItem = ((GameGridViewBinding)e.ClickedItem);
            parameters.Add(gameItem.game);
            Frame.Navigate(typeof(GameStreamsPage), parameters);
        }

        private void featuredStreamPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            featuredStreamPlayer = (MediaElement)sender;
            streamerObject = new StreamerObject(Dispatcher, featuredStreamPlayer);
        }

        private void featuredStreamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            //streamerObject.mediaElement_CurrentStateChanged(sender, e);
        }

        private void featuredStreamPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //streamerObject.mediaElement_MediaEnded(sender, e);
        }

        private void featuredStreamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //streamerObject.mediaElement_MediaFailed(sender, e);
        }

        private void mainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid = (Grid)sender;
        }

        private async void backStreamButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStreamIndex--;
            forwardStreamButton.IsEnabled = true;
            if (selectedStreamIndex == 0)
            {
                backStreamButton.IsEnabled = false;
            }
            streamerObject.Stop();
            featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
            featuredDescritpionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);

            if (videoPlaying)
            {
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
            }
        }

        private async void forwardStreamButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStreamIndex++;
            backStreamButton.IsEnabled = true;
            if (selectedStreamIndex == featuredStreams.Count - 1)
            {
                forwardStreamButton.IsEnabled = false;
            }
            streamerObject.Stop();
            featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
            featuredDescritpionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);
            if (videoPlaying)
            {
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
            }
        }

        private void backStreamButton_Loaded(object sender, RoutedEventArgs e)
        {
            backStreamButton = (AppBarButton)sender;
        }

        private void forwardStreamButton_Loaded(object sender, RoutedEventArgs e)
        {
            forwardStreamButton = (AppBarButton)sender;
        }

        private async void streamStreamButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            parameters.Add(featuredStreams[selectedStreamIndex].stream);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private async void pausePlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (videoPlaying)
            {
                streamerObject.Stop();
                videoPlaying = false;
                ((SymbolIcon)pausePlayButton.Icon).Symbol = Symbol.Play;
            }
            else
            {
                qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
                videoPlaying = true;
                ((SymbolIcon)pausePlayButton.Icon).Symbol = Symbol.Pause;
            }
        }

        private void pausePlayButton_Loaded(object sender, RoutedEventArgs e)
        {
            pausePlayButton = sender as AppBarButton;
        }

        private void channelStreamButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            parameters.Add(featuredStreams[selectedStreamIndex].stream.channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage), twixel);
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage), twixel);
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage), twixel);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            // Do Nothing
        }

        private void featuredStreamPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
