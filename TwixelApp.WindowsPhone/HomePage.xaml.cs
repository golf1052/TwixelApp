using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

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
        int selectedStreamIndex;
        Dictionary<AppConstants.Quality, Uri> qualities;
        bool videoPlaying = false;
        StreamerObject streamerObject;
        bool streamIsOffline = false;
        bool streamDoneLoading = false;
        SearchFlyout searchFlyout;

        public HomePage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
        }

        void Current_Resuming(object sender, object e)
        {
            AppConstants.DeterminePreferredQuality();
            if (videoPlaying)
            {
                AppConstants.PlayPreferredQuality(qualities, AppConstants.preferredQuality, streamerObject);
            }
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            streamerObject.Stop();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AppConstants.DeterminePreferredQuality();
            await AppConstants.SetText("Twixel");
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
                        AppConstants.PlayPreferredQuality(qualities, AppConstants.preferredQuality, streamerObject);
                        videoPlaying = true;
                    }
                }
            }
            else
            {
                await AppConstants.ShowError("Could not load featured streams.\nError Code: " + AppConstants.twixel.ErrorString);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
            streamerObject.StreamerObjectErrorEvent -= streamerObject_StreamerObjectErrorEvent;
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        public static string FixDescription(string description)
        {
            string[] split = description.Split('\n');
            return Regex.Replace(split[0], "<.*?>", string.Empty);
        }

        private void featuredStreamPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            streamerObject = new StreamerObject(Dispatcher, featuredStreamPlayer, PlayPauseAction);
            streamerObject.StreamerObjectErrorEvent += streamerObject_StreamerObjectErrorEvent;
            streamerObject.OnNavigatedTo("Twixel", "Twixel");
            Unloaded += HomePage_Unloaded;
        }

        void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            streamerObject.OnUnload();
        }

        async void streamerObject_StreamerObjectErrorEvent(object source, StreamerObjectErrorEventArgs e)
        {
            MessageDialog errorMessage = new MessageDialog("Uh...something went wrong...\nDetailed info: " + e.ErrorString);
            await errorMessage.ShowAsync();
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
                    AppConstants.PlayPreferredQuality(qualities, AppConstants.preferredQuality, streamerObject);
                    videoPlaying = true;
                    ((SymbolIcon)pausePlayButton.Icon).Symbol = Symbol.Pause;
                }
            }
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
                AppConstants.PlayPreferredQuality(qualities, AppConstants.preferredQuality, streamerObject);
            }
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

        private void channelStreamButton_Click(object sender, RoutedEventArgs e)
        {
            //if (streamDoneLoading)
            //{
            //    List<object> parameters = new List<object>();
            //    parameters.Add(featuredStreams[selectedStreamIndex].stream.channel);
            //    Frame.Navigate(typeof(ChannelPage), parameters);
            //}
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
                AppConstants.PlayPreferredQuality(qualities, AppConstants.preferredQuality, streamerObject);
            }
        }

        private void featuredStreamPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaOpened(sender, e);
        }

        private void featuredStreamPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_BufferingProgressChanged(sender, e);
        }

        private async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double phoneWidth = featuredPivotItem.ActualWidth;
            double phoneHeight = featuredPivotItem.ActualHeight;
            double ratio = 1920.0 / 1080.0;
            if (phoneWidth < phoneHeight)
            {
                // Portrait
                featuredStreamPlayer.Width = phoneWidth - 2;
                featuredStreamPlayer.Height = featuredStreamPlayer.Width / ratio;
                await StatusBar.GetForCurrentView().ShowAsync();
            }
            else
            {
                // Landscape
                double controlHeight = 56;
                double descriptionHeight;
                if (featuredDescriptionTextBlock != null)
                {
                    descriptionHeight = featuredDescriptionTextBlock.ActualHeight;
                }
                else
                {
                    descriptionHeight = 14;
                }
                featuredStreamPlayer.Height = phoneHeight - controlHeight - descriptionHeight;
                featuredStreamPlayer.Width = featuredStreamPlayer.Height * ratio;
                await StatusBar.GetForCurrentView().HideAsync();
            }
        }

        private void topGamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            topGamesGridView.ItemsSource = topGamesCollection;
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
                //Frame.Navigate(typeof(UserPage));
            }
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void searchButton_Loaded(object sender, RoutedEventArgs e)
        {
            searchFlyout = new SearchFlyout(searchBox, startSearchButton, searchComboBox, Frame);
        }
    }
}
