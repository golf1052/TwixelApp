﻿using System;
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
using WinRTXamlToolkit.Controls.Extensions;

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
            twixel = (Twixel)parameters[0];
            channel = (Channel)parameters[1];
            user = await twixel.RetrieveUser(channel.name);

            if (AppConstants.ActiveUser != null)
            {
                if (AppConstants.ActiveUser.authorized)
                {
                    userButton.Content = AppConstants.ActiveUser.displayName;
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

            pageLoaded = true;

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

            videos = await twixel.RetrieveVideos(channel.name, 100, false);

            if (videos != null)
            {
                foreach (Video video in videos)
                {
                    videosCollection.Add(new VideosGridViewBinding(video));
                }
            }
            else
            {
                AppConstants.ShowError("Could not pull channel videos.\nError Code: " + twixel.ErrorString);
            }
        }

        async void LoadMoreVideos()
        {
            if (!endOfList)
            {
                currentlyPullingVideos = true;
                loadingVideosStatusBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                videos = await twixel.RetrieveTopVideos(true);
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
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;
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
