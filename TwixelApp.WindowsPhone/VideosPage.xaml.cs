using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

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
        SearchFlyout searchFlyout;

        public VideosPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
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
            await AppConstants.SetText("fetching videos");
            StatusBar statusBar = AppConstants.GetStatusBar();
            statusBar.ProgressIndicator.ProgressValue = null;
            await statusBar.ProgressIndicator.ShowAsync();
            weekRadioButton.IsEnabled = false;
            monthRadioButton.IsEnabled = false;
            allRadioButton.IsEnabled = false;
            videos = await AppConstants.twixel.RetrieveTopVideos(100, "", period);
            foreach (Video video in videos)
            {
                videosCollection.Add(new VideosGridViewBinding(video));
            }
            currentlyPullingVideos = false;
            await AppConstants.SetText("Videos");
            weekRadioButton.IsEnabled = true;
            monthRadioButton.IsEnabled = true;
            allRadioButton.IsEnabled = true;
        }

        async void LoadMoreVideos()
        {
            if (!endOfList)
            {
                currentlyPullingVideos = true;
                await AppConstants.SetText("fetching videos");
                StatusBar statusBar = AppConstants.GetStatusBar();
                statusBar.ProgressIndicator.ProgressValue = null;
                await statusBar.ProgressIndicator.ShowAsync();
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
                await AppConstants.SetText("Videos");
                weekRadioButton.IsEnabled = true;
                monthRadioButton.IsEnabled = true;
                allRadioButton.IsEnabled = true;
            }
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

        private async void videoGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideosGridViewBinding videoItem = (VideosGridViewBinding)e.ClickedItem;
            LauncherOptions options = new LauncherOptions();
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
                //Frame.Navigate(typeof(UserPage));
            }
        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void searchButton_Loaded(object sender, RoutedEventArgs e)
        {
            searchFlyout = new SearchFlyout(searchBox, startSearchButton, searchComboBox, Frame);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
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
    }
}
