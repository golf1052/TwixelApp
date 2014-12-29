using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelPage : Page
    {
        Channel channel;
        User user;
        ObservableCollection<VideosGridViewBinding> videosCollection = new ObservableCollection<VideosGridViewBinding>();
        List<Video> videos = new List<Video>();
        bool pageLoaded = false;
        bool currentlyPullingVideos = false;
        bool endOfList = false;

        ScrollViewer videoScrollViewer;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters = (List<object>)e.Parameter;
            channel = (Channel)parameters[0];
            user = await AppConstants.twixel.RetrieveUser(channel.name);

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

            pageLoaded = true;

            displayNameBlock.Text = channel.displayName;
            if (channel.game != null)
            {
                channelGame.Text = channel.game;
            }
            if (channel.primaryTeamDisplayName != null)
            {
                channelTeam.Text = channel.primaryTeamDisplayName;
            }
            else
            {
                teamStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (channel.profileBanner != null)
            {
                BitmapImage image = new BitmapImage(channel.profileBanner.url);
                bannerImage.Source = image;
            }
            else if (channel.banner != null)
            {
                BitmapImage image = new BitmapImage(channel.banner.url);
                bannerImage.Source = image;
            }

            videos = await AppConstants.twixel.RetrieveVideos(channel.name, 100, false);

            if (videos != null)
            {
                foreach (Video video in videos)
                {
                    videosCollection.Add(new VideosGridViewBinding(video));
                }
            }
            else
            {
                await AppConstants.ShowError("Could not pull channel videos.\nError Code: " + AppConstants.twixel.ErrorString);
            }
        }

        async void LoadMoreVideos()
        {
            if (!endOfList)
            {
                currentlyPullingVideos = true;
                loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                videos = await AppConstants.twixel.RetrieveTopVideos(true);
                if (videos.Count == 0)
                {
                    endOfList = true;
                }
                foreach (Video video in videos)
                {
                    videosCollection.Add(new VideosGridViewBinding(video));
                }
                currentlyPullingVideos = false;
                loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void videosGridView_Loaded(object sender, RoutedEventArgs e)
        {
            videosGridView.ItemsSource = videosCollection;
            videoScrollViewer = videosGridView.GetFirstDescendantOfType<ScrollViewer>();
            videoScrollViewer.ViewChanged += videoScrollViewer_ViewChanged;
        }

        void videoScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (pageLoaded)
            {
                if (videoScrollViewer.ScrollableWidth == videoScrollViewer.HorizontalOffset)
                {
                    if (!currentlyPullingVideos)
                    {
                        LoadMoreVideos();
                    }
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void streamButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            TwixelAPI.Stream stream;
            try
            {
                stream = await AppConstants.twixel.RetrieveStream(channel.name);
            }
            catch
            {
                streamButton.IsEnabled = false;
                streamButton.Label = "Stream Offline";
                return;
            }
            parameters.Add(stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private async void videosGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideosGridViewBinding videoItem = (VideosGridViewBinding)e.ClickedItem;
            LauncherOptions options = new LauncherOptions();
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;
            await Launcher.LaunchUriAsync(videoItem.Url.url, options);
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
    }
}
