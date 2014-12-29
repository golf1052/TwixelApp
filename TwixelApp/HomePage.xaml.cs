using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        ObservableCollection<GameGridViewBinding> topGamesCollection;

        List<Game> justFetchedTopGames = new List<Game>();

        List<FeaturedStream> featuredStreams = new List<FeaturedStream>();
        GridView topGamesGridView;
        AppBarButton backStreamButton;
        AppBarButton forwardStreamButton;
        AppBarButton pausePlayButton;
        AppBarButton streamStreamButton;
        AppBarButton channelStreamButton;
        TextBlock featuredGameTextBlock;
        Grid streamGrid;
        MediaElement featuredStreamPlayer;
        TextBlock featuredDescriptionTextBlock;
        int selectedStreamIndex;
        Dictionary<AppConstants.Quality, Uri> qualities;

        bool videoPlaying = false;

        StreamerObject streamerObject;

        TextBlock streamOfflineTextBlock;
        bool streamIsOffline = false;

        bool streamDoneLoading = false;

        public HomePage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
        }

        private void featuredStreamPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            featuredStreamPlayer = (MediaElement)sender;
            streamerObject = new StreamerObject(Dispatcher, featuredStreamPlayer, PlayPauseAction);
            streamerObject.StreamerObjectErrorEvent += streamerObject_StreamerObjectErrorEvent;
            streamerObject.OnNavigatedTo("Twixel", "Twixel");
            Unloaded += HomePage_Unloaded;
        }

        async void streamerObject_StreamerObjectErrorEvent(object source, StreamerObjectErrorEventArgs e)
        {
            MessageDialog errorMessage = new MessageDialog("Uh...something went wrong...\nDetailed info: " + e.ErrorString);
            await errorMessage.ShowAsync();
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

            topGamesCollection = new ObservableCollection<GameGridViewBinding>();
            justFetchedTopGames = new List<Game>();
            justFetchedTopGames = await AppConstants.twixel.RetrieveTopGames(10, false);

            if (justFetchedTopGames != null)
            {
                foreach (Game game in justFetchedTopGames)
                {
                    topGamesCollection.Add(new GameGridViewBinding(game));
                }
            }
            else
            {
                await AppConstants.ShowError("Could not load top games.\nError Code: " + AppConstants.twixel.ErrorString);
            }

            featuredStreams = await AppConstants.twixel.RetrieveFeaturedStreams(5, false);
            if (featuredStreams != null)
            {
                if (featuredStreams.Count > 0)
                {
                    selectedStreamIndex = 0;
                    backStreamButton.IsEnabled = false;
                    featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
                    featuredDescriptionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
                    qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);

                    if (qualities == null)
                    {
                        streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        streamIsOffline = true;
                        pausePlayButton.IsEnabled = false;
                    }
                    else
                    {
                        streamerObject.SetTrackTitle(featuredStreams[selectedStreamIndex].stream.channel.displayName, featuredDescriptionTextBlock.Text);
                        streamerObject.SetThumbnail(featuredStreams[selectedStreamIndex].stream.channel.logo.urlString);
                        AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
                        videoPlaying = true;
                    }
                }
            }
            else
            {
                await AppConstants.ShowError("Could not load featured streams.\nError Code: " + AppConstants.twixel.ErrorString);
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
            streamerObject.StreamerObjectErrorEvent -= streamerObject_StreamerObjectErrorEvent;
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
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

        private void topGamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            topGamesGridView = sender as GridView;
            topGamesGridView.ItemsSource = topGamesCollection;
        }

        private void featuredGameTitle_Loaded(object sender, RoutedEventArgs e)
        {
            featuredGameTextBlock = (TextBlock)sender;
        }

        private void featuredGameText_Loaded(object sender, RoutedEventArgs e)
        {
            featuredDescriptionTextBlock = (TextBlock)sender;
        }

        private void topGamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (streamDoneLoading)
            {
                List<object> parameters = new List<object>();
                GameGridViewBinding gameItem = ((GameGridViewBinding)e.ClickedItem);
                parameters.Add(gameItem.game);
                Frame.Navigate(typeof(GameStreamsPage), parameters);
            }
        }

        void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            streamerObject.OnUnload();
        }

        private void featuredStreamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (featuredStreamPlayer.CurrentState == MediaElementState.Playing)
            {
                streamDoneLoading = true;
                if (selectedStreamIndex != 0)
                {
                    backStreamButton.IsEnabled = true;
                }
                streamStreamButton.IsEnabled = true;
                pausePlayButton.IsEnabled = true;
                channelStreamButton.IsEnabled = true;
                if (selectedStreamIndex != 4)
                {
                    forwardStreamButton.IsEnabled = true;
                }
                topGamesGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            streamerObject.mediaElement_CurrentStateChanged(sender, e);
        }

        private void featuredStreamPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaEnded(sender, e);
        }

        private void featuredStreamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaFailed(sender, e);
        }

        private async void backStreamButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStreamIndex--;
            forwardStreamButton.IsEnabled = true;
            streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            streamIsOffline = false;
            pausePlayButton.IsEnabled = true;

            if (selectedStreamIndex == 0)
            {
                backStreamButton.IsEnabled = false;
            }
            streamerObject.Stop();
            featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
            featuredDescriptionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);

            if (qualities == null)
            {
                streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                streamIsOffline = true;
                pausePlayButton.IsEnabled = false;
            }

            if (videoPlaying)
            {
                streamerObject.SetTrackTitle(featuredStreams[selectedStreamIndex].stream.channel.displayName, featuredDescriptionTextBlock.Text);
                streamerObject.SetThumbnail(featuredStreams[selectedStreamIndex].stream.channel.logo.urlString);
                AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
            }
        }

        private async void forwardStreamButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStreamIndex++;
            backStreamButton.IsEnabled = true;
            streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            streamIsOffline = false;
            pausePlayButton.IsEnabled = true;
            if (selectedStreamIndex == featuredStreams.Count - 1)
            {
                forwardStreamButton.IsEnabled = false;
            }
            streamerObject.Stop();
            featuredGameTextBlock.Text = featuredStreams[selectedStreamIndex].stream.channel.displayName + " playing " + featuredStreams[selectedStreamIndex].stream.game;
            featuredDescriptionTextBlock.Text = FixDescription(featuredStreams[selectedStreamIndex].text);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);

            if (qualities == null)
            {
                streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                streamIsOffline = true;
                pausePlayButton.IsEnabled = false;
            }

            if (videoPlaying)
            {
                streamerObject.SetTrackTitle(featuredStreams[selectedStreamIndex].stream.channel.displayName, featuredDescriptionTextBlock.Text);
                streamerObject.SetThumbnail(featuredStreams[selectedStreamIndex].stream.channel.logo.urlString);
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
            parameters.Add(featuredStreams[selectedStreamIndex].stream);
            qualities = await AppConstants.GetQualities(featuredStreams[selectedStreamIndex].stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void pausePlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPauseAction();
        }

        public async void PlayPauseAction()
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

                if (qualities == null)
                {
                    streamOfflineTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    streamIsOffline = true;
                    pausePlayButton.IsEnabled = false;
                }
                else
                {
                    AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
                    videoPlaying = true;
                    ((SymbolIcon)pausePlayButton.Icon).Symbol = Symbol.Pause;
                }
            }
        }

        private void pausePlayButton_Loaded(object sender, RoutedEventArgs e)
        {
            pausePlayButton = sender as AppBarButton;
        }

        private void channelStreamButton_Click(object sender, RoutedEventArgs e)
        {
            if (streamDoneLoading)
            {
                List<object> parameters = new List<object>();
                parameters.Add(featuredStreams[selectedStreamIndex].stream.channel);
                Frame.Navigate(typeof(ChannelPage), parameters);
            }
        }

        private void gamesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamesPage));
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LiveStreamsPage));
        }

        private void videosButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideosPage));
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            // Do Nothing
        }

        private void featuredStreamPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void streamOfflineTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            streamOfflineTextBlock = sender as TextBlock;
        }

        private void streamGrid_Loaded(object sender, RoutedEventArgs e)
        {
            streamGrid = (Grid)sender;
        }

        private void sectionGrid_Loaded_1(object sender, RoutedEventArgs e)
        {
            //Grid grid = sender as Grid;
            //grid.Margin = new Thickness(0, mainGrid.ActualHeight - 275 - (mainGrid.ActualHeight * (2.0/3.0)), 0, 0);
        }

        private void searchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            if (streamDoneLoading)
            {
                List<object> parameters = new List<object>();
                parameters.Add(searchBox.QueryText);
                Frame.Navigate(typeof(SearchStreamsPage), parameters);
            }
        }

        private void headerImage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void mainHub_Loaded(object sender, RoutedEventArgs e)
        {
            mainHub.Focus(FocusState.Programmatic);
        }

        private void featuredStreamPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaOpened(sender, e);
        }

        private void featuredStreamPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_BufferingProgressChanged(sender, e);
        }

        private void streamStreamButton_Loaded(object sender, RoutedEventArgs e)
        {
            streamStreamButton = sender as AppBarButton;
        }

        private void channelStreamButton_Loaded(object sender, RoutedEventArgs e)
        {
            channelStreamButton = sender as AppBarButton;
        }
    }
}
