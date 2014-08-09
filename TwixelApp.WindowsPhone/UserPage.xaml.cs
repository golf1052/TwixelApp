using System;
using System.Collections.Generic;
using System.IO;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using TwixelAPI;
using TwixelApp.Constants;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserPage : Page
    {
        Twixel twixel;
        User user;
        Channel channel;
        TwixelAPI.Stream stream;
        bool videoPlaying = false;

        Dictionary<AppConstants.Quality, Uri> qualities;

        StreamerObject streamerObject;

        public UserPage()
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

            if (twixel.users.Count != 0)
            {
                if (twixel.users[0].authorized)
                {
                    user = twixel.users[0];
                    userNameBlock.Text = user.displayName;
                    BitmapImage bitmapImage = new BitmapImage(user.logo.url);
                    profileImage.Source = bitmapImage;

                    channel = await user.RetrieveChannel(twixel);
                    statusTextBox.Text = channel.status;
                    gameTextBox.Text = channel.game;

                    qualities = await AppConstants.GetQualities(channel.name);

                    await PlayStream();
                }
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
            stream = await twixel.RetrieveStream(channel.name);
            videoPlaying = true;
            liveViewersBlock.Text = stream.viewers.ToString();
            totalViewersBlock.Text = channel.views.ToString();
            followersBlock.Text = channel.followers.ToString();
            return true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
            Application.Current.Suspending -= Current_Suspending;
            Application.Current.Resuming -= Current_Resuming;
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage), twixel);
        }

        private async void updateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            channel = await user.UpdateChannel(statusTextBox.Text, gameTextBox.Text, twixel);
            statusTextBox.Text = channel.status;
            gameTextBox.Text = channel.game;
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

        private void userButton_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing
        }

        private void streamPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            streamerObject = new StreamerObject(Dispatcher, streamPlayer);
        }

        private async void playPauseButton_Click(object sender, RoutedEventArgs e)
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
            parameters.Add(twixel);
            parameters.Add(channel);
            Frame.Navigate(typeof(ChannelPage), parameters);
        }
    }
}
