using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Imaging;
using TwixelAPI;
using TwixelApp.Constants;
using Newtonsoft.Json.Linq;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelPage : Page
    {
        Twixel twixel;
        Channel channel;
        User user;
        ObservableCollection<VideosGridViewBinding> videosCollection = new ObservableCollection<VideosGridViewBinding>();
        ObservableCollection<FollowersGridViewBinding> followersCollection = new ObservableCollection<FollowersGridViewBinding>();
        List<Video> videos = new List<Video>();
        List<User> followers = new List<User>();

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters = (List<object>)e.Parameter;
            twixel = (Twixel)parameters[0];
            channel = (Channel)parameters[1];
            user = await twixel.CreateUser(channel.name);

            if (twixel.users.Count > 0)
            {
                if (twixel.users[0].authorized)
                {
                    userButton.Content = twixel.users[0].displayName;
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

            displayNameBlock.Text = channel.displayName;
            channelGame.Text = channel.game;
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

            if (videos.Count == 0)
            {
                videos = await twixel.RetrieveVideos(channel.name, false);
            }
            foreach (Video video in videos)
            {
                videosCollection.Add(new VideosGridViewBinding(video));
            }

            if (followers.Count == 0)
            {
                followers = await twixel.RetrieveFollowers(user.name, 100);
            }
            foreach (User follower in followers)
            {
                followersCollection.Add(new FollowersGridViewBinding(follower));
            }
        }

        private void videosGridView_Loaded(object sender, RoutedEventArgs e)
        {
            videosGridView.ItemsSource = videosCollection;
        }

        private void followersGridView_Loaded(object sender, RoutedEventArgs e)
        {
            followersGridView.ItemsSource = followersCollection;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void streamButton_Click(object sender, RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(twixel);
            TwixelAPI.Stream stream = await twixel.RetrieveStream(channel.name);
            parameters.Add(stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private async void videosGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideosGridViewBinding videoItem = (VideosGridViewBinding)e.ClickedItem;
            LauncherOptions options = new LauncherOptions();
            //options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;
            await Launcher.LaunchUriAsync(videoItem.Url.url, options);
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
    }
}
