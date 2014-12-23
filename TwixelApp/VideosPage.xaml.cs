using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideosPage : Page
    {
        ObservableCollection<VideosGridViewBinding> videosCollection;
        List<Video> videos;
        bool pageLoaded = false;
        bool currentlyPullingVideos = false;
        bool endOfList = false;
        ScrollViewer videoScrollViewer;

        public VideosPage()
        {
            this.InitializeComponent();
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

            pageLoaded = true;

            videosCollection = new ObservableCollection<VideosGridViewBinding>();
            videos = new List<Video>();

            await GetVideos(TwitchConstants.Period.Week);
        }

        async Task GetVideos(TwitchConstants.Period period)
        {
            videos.Clear();
            videosCollection.Clear();
            endOfList = false;
            currentlyPullingVideos = true;
            loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            weekRadioButton.IsEnabled = false;
            monthRadioButton.IsEnabled = false;
            allRadioButton.IsEnabled = false;
            videos = await AppConstants.twixel.RetrieveTopVideos(100, "", period);
            foreach (Video video in videos)
            {
                videosCollection.Add(new VideosGridViewBinding(video));
            }
            currentlyPullingVideos = false;
            loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            weekRadioButton.IsEnabled = true;
            monthRadioButton.IsEnabled = true;
            allRadioButton.IsEnabled = true;
        }

        async void LoadMoreVideos()
        {
            if (!endOfList)
            {
                currentlyPullingVideos = true;
                loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                weekRadioButton.IsEnabled = false;
                monthRadioButton.IsEnabled = false;
                allRadioButton.IsEnabled = false;
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
                weekRadioButton.IsEnabled = true;
                monthRadioButton.IsEnabled = true;
                allRadioButton.IsEnabled = true;
            }
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
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
            // Do nothing
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

        private async void weekRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (pageLoaded)
            {
                await GetVideos(TwitchConstants.Period.Week);
            }
        }

        private async void monthRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            await GetVideos(TwitchConstants.Period.Month);
        }

        private async void allRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            await GetVideos(TwitchConstants.Period.All);
        }

        private async void videoGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideosGridViewBinding videoItem = (VideosGridViewBinding)e.ClickedItem;
            LauncherOptions options = new LauncherOptions();
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;
            await Launcher.LaunchUriAsync(videoItem.Url.url, options);
        }

        private void videoGridView_Loaded(object sender, RoutedEventArgs e)
        {
            videoGridView.ItemsSource = videosCollection;
            videoScrollViewer = videoGridView.GetFirstDescendantOfType<ScrollViewer>();
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
    }
}
