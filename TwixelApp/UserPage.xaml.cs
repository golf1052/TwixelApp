using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TwixelAPI;
using TwixelApp.Constants;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserPage : Page
    {
        User user;
        Channel channel;
        TwixelAPI.Stream stream;
        bool justLaunchedPage = true;
        bool videoPlaying = false;
        ChatWindow chatWindow;

        Dictionary<AppConstants.Quality, Uri> qualities;

        StreamerObject streamerObject;

        VolumeFlyout volumeFlyout;

        TextBox statusTextBox;
        TextBox gameTextBox;
        Button updateStatusButton;
        Grid chatGrid;
        ListView chatView;
        TextBox chatBox;
        Button chatSendButton;
        MediaElement streamPlayer;
        TextBlock liveViewersBlock;
        TextBlock totalViewersBlock;
        TextBlock followersBlock;
        AppBarButton playPauseButton;
        AppBarButton channelButton;
        AppBarButton volumeButton;
        Button muteButton;
        Slider volumeSlider;
        TextBlock streamOfflineBlock;
        bool loadedMainHubSection = false;
        bool loadedVolumeSlider = false;

        GridView followingGridView;
        ObservableCollection<GameStreamsGridViewBinding> followedLiveStreamsCollection;
        List<TwixelAPI.Stream> streamsFollowed;

        ListView followedProfilesListView;
        List<Channel> channelsFollowed;
        ObservableCollection<FollowersGridViewBinding> followedProfilesCollection;
        ListView blockedUsersListView;
        List<User> blockedUsers;
        ObservableCollection<FollowersGridViewBinding> blockedUsersCollection;
        ListView channelEditorsListView;
        List<User> channelEditors;
        ObservableCollection<FollowersGridViewBinding> channelEditorsCollection;
        TextBox streamKeyTextBox;
        Button resetKeyButton;
        Button showHideKeyButton;

        public UserPage()
        {
            this.InitializeComponent();
            followedLiveStreamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            streamsFollowed = new List<TwixelAPI.Stream>();
            channelsFollowed = new List<Channel>();
            blockedUsers = new List<User>();
            channelEditors = new List<User>();

            followedProfilesCollection = new ObservableCollection<FollowersGridViewBinding>();
            blockedUsersCollection = new ObservableCollection<FollowersGridViewBinding>();
            channelEditorsCollection = new ObservableCollection<FollowersGridViewBinding>();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
        }

        private void streamPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            streamPlayer = sender as MediaElement;
            streamerObject = new StreamerObject(Dispatcher, streamPlayer, PlayPauseAction);
            streamerObject.StreamerObjectErrorEvent += streamerObject_StreamerObjectErrorEvent;
            streamerObject.OnNavigatedTo("Twixel", "Twixel");
            Unloaded += UserPage_Unloaded;
            ElementsLoaded();
        }

        async void streamerObject_StreamerObjectErrorEvent(object source, StreamerObjectErrorEventArgs e)
        {
            MessageDialog errorMessage = new MessageDialog("Uh...something went wrong...\nDetailed info: " + e.ErrorString);
            await errorMessage.ShowAsync();
            Frame.GoBack();
        }

        void UserPage_Unloaded(object sender, RoutedEventArgs e)
        {
            streamerObject.OnUnload();
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
                    user = AppConstants.ActiveUser;
                    userNameBlock.Text = user.displayName;
                    userButton.Content = AppConstants.ActiveUser.displayName;
                    BitmapImage bitmapImage = new BitmapImage(user.logo.url);
                    profileImage.Source = bitmapImage;

                    channel = await user.RetrieveChannel();
                    statusTextBox.Text = channel.status;
                    if (channel.game != null)
                    {
                        gameTextBox.Text = channel.game;
                    }
                    else
                    {
                        gameTextBox.Text = "";
                    }

                    qualities = await AppConstants.GetQualities(channel.name);

                    await PlayStream();
                }
            }

            justLaunchedPage = false;
            ElementsLoaded();

            streamsFollowed = await user.RetrieveOnlineFollowedStreams();
            foreach (TwixelAPI.Stream stream in streamsFollowed)
            {
                followedLiveStreamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }

            channelsFollowed = await user.RetrieveFollowing(100);
            foreach (Channel followedChannel in channelsFollowed)
            {
                if (followedChannel.logo != null)
                {
                    followedProfilesCollection.Add(new FollowersGridViewBinding(followedChannel.logo.urlString, followedChannel.displayName));
                }
                else
                {
                    followedProfilesCollection.Add(new FollowersGridViewBinding(null, followedChannel.displayName));
                }
                followedProfilesCollection[followedProfilesCollection.Count - 1].Channel = followedChannel;
            }

            blockedUsers = await user.RetrieveBlockedUsers();
            foreach (User blockedUser in blockedUsers)
            {
                blockedUsersCollection.Add(new FollowersGridViewBinding(blockedUser));
            }

            channelEditors = await user.RetrieveChannelEditors();
            foreach (User channelEditor in channelEditors)
            {
                channelEditorsCollection.Add(new FollowersGridViewBinding(channelEditor));
            }
        }

        async void ElementsLoaded()
        {
            if (statusTextBox != null &&
                gameTextBox != null &&
                updateStatusButton != null &&
                chatGrid != null &&
                chatView != null &&
                chatBox != null &&
                chatSendButton != null &&
                streamPlayer != null &&
                liveViewersBlock != null &&
                totalViewersBlock != null &&
                followersBlock != null &&
                playPauseButton != null &&
                channelButton != null &&
                volumeButton != null &&
                streamOfflineBlock != null &&
                loadedMainHubSection == false &&
                justLaunchedPage == false)
            {
                loadedMainHubSection = true;
                chatWindow = new ChatWindow(Dispatcher, channel.name, chatGrid, chatView, chatBox, chatSendButton);
                await chatWindow.LoadChatWindow();
            }
        }

        void LoadVolumeFlyout()
        {
            if (muteButton != null &&
                volumeSlider != null &&
                loadedVolumeSlider == false)
            {
                loadedVolumeSlider = true;
                volumeFlyout = new VolumeFlyout(volumeSlider, muteButton, volumeButton, streamPlayer);
            }
        }

        async Task<bool> PlayStream()
        {
            streamOfflineBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (qualities == null)
            {
                // stream offline
                streamOfflineBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                liveViewersBlock.Text = "Offline";
                playPauseButton.Label = "Play";
                ((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Play;
                totalViewersBlock.Text = channel.views.ToString();
                followersBlock.Text = channel.followers.ToString();
                return false;
            }

            AppConstants.PlayPreferredQuality(qualities, AppConstants.Quality.Source, streamerObject);
            stream = await AppConstants.twixel.RetrieveStream(channel.name);
            videoPlaying = true;
            liveViewersBlock.Text = stream.viewers.ToString();
            totalViewersBlock.Text = channel.views.ToString();
            followersBlock.Text = channel.followers.ToString();
            streamerObject.SetTrackTitle(stream.channel.displayName, channel.status);
            streamerObject.SetThumbnail(stream.channel.logo.urlString);
            return true;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
            if (chatWindow.client.isConnectedToChannel)
            {
                await chatWindow.client.SendPart();
            }
            streamerObject.StreamerObjectErrorEvent -= streamerObject_StreamerObjectErrorEvent;
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private async void updateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            channel = await user.UpdateChannel(statusTextBox.Text, gameTextBox.Text);
            statusTextBox.Text = channel.status;
            gameTextBox.Text = channel.game;
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

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPauseAction();
        }

        public async void PlayPauseAction()
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
                if (await PlayStream())
                {
                    playPauseButton.Label = "Pause";
                    ((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Pause;
                }
            }
        }

        private void channelButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }

        private void statusTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            statusTextBox = sender as TextBox;
            ElementsLoaded();
        }

        private void gameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            gameTextBox = sender as TextBox;
            ElementsLoaded();
        }

        private void updateStatusButton_Loaded(object sender, RoutedEventArgs e)
        {
            updateStatusButton = sender as Button;
            ElementsLoaded();
        }

        private void chatGrid_Loaded(object sender, RoutedEventArgs e)
        {
            chatGrid = sender as Grid;
            ElementsLoaded();
        }

        private void chatView_Loaded(object sender, RoutedEventArgs e)
        {
            chatView = sender as ListView;
            ElementsLoaded();
        }

        private void chatBox_Loaded(object sender, RoutedEventArgs e)
        {
            chatBox = sender as TextBox;
            ElementsLoaded();
        }

        private void chatSendButton_Loaded(object sender, RoutedEventArgs e)
        {
            chatSendButton = sender as Button;
            ElementsLoaded();
        }

        private void liveViewersBlock_Loaded(object sender, RoutedEventArgs e)
        {
            liveViewersBlock = sender as TextBlock;
            ElementsLoaded();
        }

        private void totalViewersBlock_Loaded(object sender, RoutedEventArgs e)
        {
            totalViewersBlock = sender as TextBlock;
            ElementsLoaded();
        }

        private void followersBlock_Loaded(object sender, RoutedEventArgs e)
        {
            followersBlock = sender as TextBlock;
            ElementsLoaded();
        }

        private void playPauseButton_Loaded(object sender, RoutedEventArgs e)
        {
            playPauseButton = sender as AppBarButton;
            ElementsLoaded();
        }

        private void channelButton_Loaded(object sender, RoutedEventArgs e)
        {
            channelButton = sender as AppBarButton;
            ElementsLoaded();
        }

        private void volumeButton_Loaded(object sender, RoutedEventArgs e)
        {
            volumeButton = sender as AppBarButton;
            ElementsLoaded();
        }

        private void muteButton_Loaded(object sender, RoutedEventArgs e)
        {
            muteButton = sender as Button;
            LoadVolumeFlyout();
        }

        private void volumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            volumeSlider = sender as Slider;
            LoadVolumeFlyout();
        }

        private void streamOfflineBlock_Loaded(object sender, RoutedEventArgs e)
        {
            streamOfflineBlock = sender as TextBlock;
            ElementsLoaded();
        }

        private void followingGridView_Loaded(object sender, RoutedEventArgs e)
        {
            followingGridView = sender as GridView;
            followingGridView.ItemsSource = followedLiveStreamsCollection;
        }

        private async void followingGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;

            List<object> parameters = new List<object>();
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void streamPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_BufferingProgressChanged(sender, e);
        }

        private void streamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_CurrentStateChanged(sender, e);
        }

        private void streamPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaEnded(sender, e);
        }

        private void streamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaFailed(sender, e);
        }

        private void streamPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            streamerObject.mediaElement_MediaOpened(sender, e);
        }

        private void followedProfilesListView_Loaded(object sender, RoutedEventArgs e)
        {
            followedProfilesListView = sender as ListView;
            followedProfilesListView.ItemsSource = followedProfilesCollection;
        }

        private void followedProfilesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FollowersGridViewBinding profileItem = (FollowersGridViewBinding)e.ClickedItem;

            List<object> parameters = new List<object>();
            parameters.Add(profileItem.Channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }

        private void blockedUsersListView_Loaded(object sender, RoutedEventArgs e)
        {
            blockedUsersListView = sender as ListView;
            blockedUsersListView.ItemsSource = blockedUsersCollection;
        }

        private void channelEditorsListView_Loaded(object sender, RoutedEventArgs e)
        {
            channelEditorsListView = sender as ListView;
            channelEditorsListView.ItemsSource = channelEditorsCollection;
        }

        private void streamKeyTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            streamKeyTextBox = sender as TextBox;
            streamKeyTextBox.Text = user.streamKey;
        }

        private void resetKeyButton_Loaded(object sender, RoutedEventArgs e)
        {
            resetKeyButton = sender as Button;
        }

        private async void resetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            resetKeyButton.IsEnabled = false;
            streamKeyTextBox.Text = await user.ResetStreamKey();
            resetKeyButton.IsEnabled = true;
        }

        private void showHideKeyButton_Loaded(object sender, RoutedEventArgs e)
        {
            showHideKeyButton = sender as Button;
        }

        private void showHideKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (streamKeyTextBox.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                streamKeyTextBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                showHideKeyButton.Content = "Hide Key";
            }
            else
            {
                streamKeyTextBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                showHideKeyButton.Content = "Show Key";
            }
        }
    }
}
