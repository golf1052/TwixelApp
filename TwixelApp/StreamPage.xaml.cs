using System;
using System.Collections.Generic;
using System.Diagnostics;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StreamPage : Page
    {
        Stream stream;
        Dictionary<AppConstants.Quality, Uri> qualities;
        bool justLaunchedPage = true;
        bool videoPlaying = false;
        bool streamOffline = false;

        StreamerObject streamerObject;
        ChatWindow chatWindow;
        VolumeFlyout volumeFlyout;

        double screenWidth = 0;
        double screenHeight = 0;
        bool chatOpen = true;

        User user;
        List<Channel> channelsFollowed;

        enum ChatGridLocation
        {
            Side,
            Bottom
        }

        ChatGridLocation chatGridLocation = ChatGridLocation.Side;

        public StreamPage()
        {
            this.InitializeComponent();
            channelsFollowed = new List<Channel>();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            this.SizeChanged += StreamPage_SizeChanged;

            streamerObject = new StreamerObject(Dispatcher, streamPlayer, PlayPauseAction);
            streamerObject.StreamerObjectErrorEvent += streamerObject_StreamerObjectErrorEvent;
            Unloaded += StreamPage_Unloaded;
        }

        async void streamerObject_StreamerObjectErrorEvent(object source, StreamerObjectErrorEventArgs e)
        {
            MessageDialog errorMessage = new MessageDialog("Uh...something went wrong...\nDetailed info: " + e.ErrorString);
            await errorMessage.ShowAsync();
            Frame.GoBack();
        }

        void StreamPage_Unloaded(object sender, RoutedEventArgs e)
        {
            streamerObject.OnUnload();
        }

        void StreamPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            screenHeight = e.NewSize.Height;
            screenWidth = e.NewSize.Width;

            if (e.NewSize.Width <= 720)
            {
                backButton.IsCompact = true;
                playPauseButton.IsCompact = true;
                channelButton.IsCompact = true;
                followButton.IsCompact = true;
                volumeButton.IsCompact = true;
                chatButton.IsCompact = true;
                streamViewersStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                streamQualities.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (chatOpen)
                {
                    bottomChatGrid.Height = screenHeight - (screenWidth * ((double)streamPlayer.NaturalVideoHeight) / (double)streamPlayer.NaturalVideoWidth);
                    sideChatGrid.Width = 0;
                }
                if (chatGridLocation == ChatGridLocation.Side)
                {
                    chatGridLocation = ChatGridLocation.Bottom;
                    sideChatGrid.Children.Remove(chatGrid);
                    bottomChatGrid.Children.Add(chatGrid);
                    chatGrid.UpdateLayout();
                    chatView.UpdateLayout();
                }
            }
            else
            {
                backButton.IsCompact = false;
                playPauseButton.IsCompact = false;
                channelButton.IsCompact = false;
                followButton.IsCompact = false;
                volumeButton.IsCompact = false;
                chatButton.IsCompact = false;
                streamViewersStackPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                streamQualities.Visibility = Windows.UI.Xaml.Visibility.Visible;
                if (chatOpen)
                {
                    sideChatGrid.Width = 340;
                    bottomChatGrid.Height = 0;
                }
                if (chatGridLocation == ChatGridLocation.Bottom)
                {
                    chatGridLocation = ChatGridLocation.Side;
                    bottomChatGrid.Children.Remove(chatGrid);
                    sideChatGrid.Children.Add(chatGrid);
                    chatGrid.UpdateLayout();
                    chatView.UpdateLayout();
                }
            }
        }

        void Current_Resuming(object sender, object e)
        {
            if (videoPlaying)
            {
                PlayReloadStream();
            }
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            streamerObject.Stop();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = (List<object>)e.Parameter;
            stream = (TwixelAPI.Stream)parameters[0];
            qualities = (Dictionary<AppConstants.Quality, Uri>)parameters[1];

            #region Stream Stuff
            if (CheckOffline())
            {
                return;
            }

            foreach (KeyValuePair<AppConstants.Quality, Uri> quality in qualities)
            {
                ComboBoxItem item = new ComboBoxItem();
                if (quality.Key == AppConstants.Quality.Source)
                {
                    item.Content = "Source";
                }
                else if (quality.Key == AppConstants.Quality.High)
                {
                    item.Content = "High";
                }
                else if (quality.Key == AppConstants.Quality.Medium)
                {
                    item.Content = "Medium";
                }
                else if (quality.Key == AppConstants.Quality.Low)
                {
                    item.Content = "Low";
                }
                else if (quality.Key == AppConstants.Quality.Mobile)
                {
                    item.Content = "Mobile";
                }
                else if (quality.Key == AppConstants.Quality.Chunked)
                {
                    item.Content = "Default";
                }

                streamQualities.Items.Add(item);

                if (streamQualities.Items.Count == 1)
                {
                    streamQualities.SelectedIndex = 0;
                }
            }

            streamDescription.Text = stream.channel.status;
            streamerName.Text = stream.channel.displayName;
            streamGame.Text = stream.game;
            streamViewers.Text = stream.viewers.ToString();

            streamerObject.OnNavigatedTo(stream.channel.displayName, stream.channel.status);

            if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Source")
            {
                StartStream(qualities[AppConstants.Quality.Source]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "High")
            {
                StartStream(qualities[AppConstants.Quality.High]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Medium")
            {
                StartStream(qualities[AppConstants.Quality.Medium]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Low")
            {
                StartStream(qualities[AppConstants.Quality.Low]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Mobile")
            {
                StartStream(qualities[AppConstants.Quality.Mobile]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Default")
            {
                StartStream(qualities[AppConstants.Quality.Chunked]);
            }

            justLaunchedPage = false;
            volumeFlyout = new VolumeFlyout(volumeSlider, muteButton, volumeButton, streamPlayer);
            chatWindow = new ChatWindow(Dispatcher, stream.channel.name, chatGrid, chatView, chatBox, chatSendButton);
            await chatWindow.LoadChatWindow();
            #endregion

            if (AppConstants.ActiveUser != null)
            {
                if (AppConstants.ActiveUser.authorized)
                {
                    userButton.Content = AppConstants.ActiveUser.displayName;
                    user = AppConstants.ActiveUser;
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

            if (user != null)
            {
                List<Channel> temp = await user.RetrieveFollowing(100);
                if (temp != null)
                {
                    foreach (Channel channel in temp)
                    {
                        channelsFollowed.Add(channel);
                    }
                }

                do
                {
                    temp = await user.RetrieveFollowing(true);

                    if (temp != null)
                    {
                        foreach (Channel channel in temp)
                        {
                            channelsFollowed.Add(channel);
                        }
                    }
                }
                while (temp.Count != 0);

                if (user.authorizedScopes.Contains(TwixelAPI.Constants.TwitchConstants.Scope.UserFollowsEdit))
                {
                    foreach (Channel channel in channelsFollowed)
                    {
                        if (channel.name == stream.channel.name)
                        {
                            followButton.Label = "Unfollow";
                            ((SymbolIcon)followButton.Icon).Symbol = Symbol.Clear;
                            break;
                        }
                    }
                }
                else
                {
                    followButton.IsEnabled = false;
                }
            }
            else
            {
                followButton.IsEnabled = false;
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
            if (chatWindow != null)
            {
                if (chatWindow.connectedToChat)
                {
                    await chatWindow.client.SendPart();
                }
            }

            streamerObject.StreamerObjectErrorEvent -= streamerObject_StreamerObjectErrorEvent;
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        private void streamPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaEnded(sender, e);
        }

        private void streamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaFailed(sender, e);
        }

        private void streamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_CurrentStateChanged(sender, e);
        }

        void StartStream(Uri streamUrl)
        {
            Debug.WriteLine("Starting stream");
            streamerObject.SetStreamUrl(streamUrl);
            streamerObject.SetTrackTitle(stream.channel.displayName, stream.channel.status);
            if (stream.channel.logo != null)
            {
                streamerObject.SetThumbnail(stream.channel.logo.urlString);
            }
            streamerObject.StartStream();
            videoPlaying = true;
        }

        private void streamQualities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (justLaunchedPage == false)
            {
                if (videoPlaying)
                {
                    streamerObject.Stop();

                    PlayStream();
                }
            }
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPauseAction();
        }

        public void PlayPauseAction()
        {
            if (videoPlaying)
            {
                streamerObject.Stop();
                videoPlaying = false;
                playPauseButton.Label = "Play";
                ((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Play;
            }
            else
            {
                playPauseButton.Label = "Pause";
                ((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Pause;
                PlayReloadStream();
            }
        }

        void PlayStream()
        {
            if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Source")
            {
                StartStream(qualities[AppConstants.Quality.Source]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "High")
            {
                StartStream(qualities[AppConstants.Quality.High]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Medium")
            {
                StartStream(qualities[AppConstants.Quality.Medium]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Low")
            {
                StartStream(qualities[AppConstants.Quality.Low]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Mobile")
            {
                StartStream(qualities[AppConstants.Quality.Mobile]);
            }
            else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Default")
            {
                StartStream(qualities[AppConstants.Quality.Chunked]);
            }
        }

        async void PlayReloadStream()
        {
            qualities = await AppConstants.GetQualities(stream.channel.name);

            if (CheckOffline())
            {
                return;
            }

            PlayStream();
        }

        bool CheckOffline()
        {
            if (qualities == null)
            {
                // stream offline
                streamOfflineBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                streamOffline = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AppBar_Opened(object sender, object e)
        {
            qualities = await AppConstants.GetQualities(stream.channel.name);

            if (CheckOffline())
            {
                streamOffline = true;
            }

            if (!streamOffline)
            {
                stream = await AppConstants.twixel.RetrieveStream(stream.channel.name);
                if (stream != null)
                {
                    streamDescription.Text = stream.channel.status;
                    streamerName.Text = stream.channel.displayName;
                    streamGame.Text = stream.game;
                    streamViewers.Text = stream.viewers.ToString();
                }
            }
        }

        private void channelButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(stream.channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
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

        private void chatButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatGrid.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                chatOpen = false;
                if (chatGridLocation == ChatGridLocation.Bottom)
                {
                    chatGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    bottomChatGrid.Height = 0;
                    chatButton.Label = "Open Chat";
                }
                else
                {
                    chatGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    sideChatGrid.Width = 0;
                    chatButton.Label = "Open Chat";
                }
            }
            else
            {
                chatOpen = true;
                if (chatGridLocation == ChatGridLocation.Bottom)
                {
                    bottomChatGrid.Height = screenHeight - (screenWidth * ((double)streamPlayer.NaturalVideoHeight) / (double)streamPlayer.NaturalVideoWidth);
                    chatGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    chatButton.Label = "Close Chat";
                }
                else
                {
                    chatGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    sideChatGrid.Width = 340;
                    chatButton.Label = "Close Chat";
                }
            }
        }

        private async void followButton_Click(object sender, RoutedEventArgs e)
        {
            if (followButton.Label == "Follow")
            {
                await user.FollowChannel(stream.channel.name);
                followButton.Label = "Unfollow";
                ((SymbolIcon)followButton.Icon).Symbol = Symbol.Clear;
            }
            else
            {
                await user.UnfollowChannel(stream.channel.name);
                followButton.Label = "Follow";
                ((SymbolIcon)followButton.Icon).Symbol = Symbol.Add;
            }
        }

        private void streamPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_BufferingProgressChanged(sender, e);
        }

        private void streamPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaOpened(sender, e);
        }
    }
}
