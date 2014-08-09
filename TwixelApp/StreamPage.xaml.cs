using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using TwixelAPI;
using TwixelApp.Constants;
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
        Twixel twixel;
        TwixelAPI.Stream stream;
        Dictionary<AppConstants.Quality, Uri> qualities;
        bool justLaunchedPage = true;
        bool videoPlaying = false;
        bool streamOffline = false;

        StreamerObject streamerObject;
        ChatWindow chatWindow;
        VolumeFlyout volumeFlyout;

        StackPanel stackPanel;

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

            streamerObject = new StreamerObject(Dispatcher, streamPlayer);
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
            twixel = (Twixel)parameters[0];
            stream = (TwixelAPI.Stream)parameters[1];
            qualities = (Dictionary<AppConstants.Quality, Uri>)parameters[2];

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

            streamerObject.OnNavigatedTo();

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
            chatWindow = new ChatWindow(twixel, Dispatcher, stream.channel.name, chatGrid, chatView, chatBox, chatSendButton);
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
                    userButton.Content = "Not Logged In";
                    userButton.IsEnabled = false;
                }
            }
            else
            {
                userButton.Content = "Not Logged In";
                userButton.IsEnabled = false;
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
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        private void streamPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //streamerObject.mediaElement_MediaEnded(sender, e);
        }

        private void streamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //streamerObject.mediaElement_MediaFailed(sender, e);
        }

        private void streamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            //streamerObject.mediaElement_CurrentStateChanged(sender, e);
        }

        void StartStream(Uri streamUrl)
        {
            Debug.WriteLine("Starting stream");

            streamerObject.StartStream(streamUrl);

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
            Task task = Task.Run(async () => { qualities = await AppConstants.GetQualities(stream.channel.name); });
            task.Wait();

            if (CheckOffline())
            {
                streamOffline = true;
            }

            if (!streamOffline)
            {
                stream = await twixel.RetrieveStream(stream.channel.name);
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
            parameters.Add(twixel);
            parameters.Add(stream.channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), twixel);
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
    }
}
