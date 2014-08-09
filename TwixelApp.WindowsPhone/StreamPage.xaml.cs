using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Core;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using TwixelAPI;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using SM.Media;
using SM.Media.Playlists;
using SM.Media.Segments;
using SM.Media.Web;
using TwixelApp.Constants;
using WinRTXamlToolkit.Controls.Extensions;

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
        bool justLaunchedPaged = true;
        bool videoPlaying = false;

        Client client;
        MessageHandlerListener messageListener;

        ObservableCollection<ChatListViewBinding> chatMessages = new ObservableCollection<ChatListViewBinding>();
        public bool lockToBottom = true;
        ScrollBar verticalBarBar;

        StreamerObject streamerObject;

        public StreamPage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;

            streamerObject = new StreamerObject(Dispatcher, streamPlayer);
        }

        void Current_Resuming(object sender, object e)
        {
            if (videoPlaying)
            {
                PlayStream();
            }
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            streamerObject.Stop();
        }

        async void client_Message(object source, MessageHandlerEventArgs e)
        {
            if (e.Message.Contains("376"))
            {
                await client.SendJoin();
            }

            string name = "";
            string chatMessage = "";
            if (e.Message.Contains("PRIVMSG"))
            {
                string[] splitMessage = e.Message.Split(':');
                if (splitMessage.Length > 2)
                {
                    string[] splitUsername = splitMessage[1].Split('!');
                    name = splitUsername[0];
                    string rejoined = "";
                    for (int i = 0; i < splitMessage.Length; i++)
                    {
                        if (i >= 2)
                        {
                            rejoined += splitMessage[i];
                        }
                    }
                    chatMessage = rejoined;
                    chatMessage = chatMessage.Remove(chatMessage.Length - 2, 2);
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (name != "" && chatMessage != "")
                    {
                        chatMessages.Add(new ChatListViewBinding(name, chatMessage, false));
                    }
                });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parameters = (List<object>)e.Parameter;
            twixel = (Twixel)parameters[0];
            stream = (TwixelAPI.Stream)parameters[1];
            qualities = (Dictionary<AppConstants.Quality, Uri>)parameters[2];

            #region Stream Stuff
            if (qualities == null)
            {
                // stream offline
                streamOfflineBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
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

                //streamQualities.Items.Add(item);

                //if (streamQualities.Items.Count == 1)
                //{
                //    streamQualities.SelectedIndex = 0;
                //}
            }

            //streamDescription.Text = stream.channel.status;
            //streamerName.Text = stream.channel.displayName;
            //streamGame.Text = stream.game;
            //streamViewers.Text = stream.viewers.ToString();

            streamerObject.OnNavigatedTo();

            StartStream(qualities[AppConstants.Quality.Source]);

            //if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Source")
            //{
            //    StartStream(qualities[AppConstants.Quality.Source]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "High")
            //{
            //    StartStream(qualities[AppConstants.Quality.High]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Medium")
            //{
            //    StartStream(qualities[AppConstants.Quality.Medium]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Low")
            //{
            //    StartStream(qualities[AppConstants.Quality.Low]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Mobile")
            //{
            //    StartStream(qualities[AppConstants.Quality.Mobile]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Default")
            //{
            //    StartStream(qualities[AppConstants.Quality.Chunked]);
            //}

            justLaunchedPaged = false;
            #endregion

            if (twixel.users.Count > 0)
            {
                if (twixel.users[0].authorized)
                {
                    //userButton.Content = twixel.users[0].displayName;
                }
                else
                {
                    //userButton.Content = "Not Logged In";
                    //userButton.IsEnabled = false;
                }
            }
            else
            {
                //userButton.Content = "Not Logged In";
                //userButton.IsEnabled = false;
            }

            client = new Client(stream.channel.name);
            messageListener = new MessageHandlerListener();
            client.Message += client_Message;

            if (twixel.users.Count > 0)
            {
                if (twixel.users[0].authorized)
                {
                    if (twixel.users[0].authorizedScopes.Contains(TwixelAPI.Constants.TwitchConstants.Scope.ChatLogin))
                    {
                        await client.Connect();
                        await client.Login("golf1052", twixel.users[0].accessToken);
                    }
                }
                else
                {
                    chatGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    //chatButton.Label = "Open Chat";
                    chatBox.Text = "Not logged in";
                    chatBox.IsEnabled = false;
                    chatSendButton.IsEnabled = false;
                }
            }
            else
            {
                chatGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //chatButton.Label = "Open Chat";
                chatBox.Text = "Not logged in";
                chatBox.IsEnabled = false;
                chatSendButton.IsEnabled = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            streamerObject.OnNavigatedFrom();
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
            if (justLaunchedPaged == false)
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
                //playPauseButton.Label = "Play";
                //((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Play;
            }
            else
            {
                //playPauseButton.Label = "Pause";
                //((SymbolIcon)playPauseButton.Icon).Symbol = Symbol.Pause;
                PlayStream();
            }
        }

        void PlayStream()
        {
            StartStream(qualities[AppConstants.Quality.Source]);
            //if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Source")
            //{
            //    StartStream(qualities[AppConstants.Quality.Source]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "High")
            //{
            //    StartStream(qualities[AppConstants.Quality.High]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Medium")
            //{
            //    StartStream(qualities[AppConstants.Quality.Medium]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Low")
            //{
            //    StartStream(qualities[AppConstants.Quality.Low]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Mobile")
            //{
            //    StartStream(qualities[AppConstants.Quality.Mobile]);
            //}
            //else if ((string)((ComboBoxItem)streamQualities.SelectedItem).Content == "Default")
            //{
            //    StartStream(qualities[AppConstants.Quality.Chunked]);
            //}
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AppBar_Opened(object sender, object e)
        {
            stream = await twixel.RetrieveStream(stream.channel.name);
            if (stream != null)
            {
                //streamDescription.Text = stream.channel.status;
                //streamerName.Text = stream.channel.displayName;
                //streamGame.Text = stream.game;
                //streamViewers.Text = stream.viewers.ToString();
            }
        }

        private async void chatSendButton_Click(object sender, RoutedEventArgs e)
        {
            await client.SendIRCMessage(chatBox.Text);
            chatBox.Text = "";
        }

        private void chatView_Loaded(object sender, RoutedEventArgs e)
        {
            chatView.ItemsSource = chatMessages;
            chatMessages.CollectionChanged += (s, args) => ScrollToBottom();
            var scrollViewer = chatView.GetFirstDescendantOfType<ScrollViewer>();
            var scrollBars = scrollViewer.GetDescendantsOfType<ScrollBar>().ToList();
            var verticalBar = scrollBars.FirstOrDefault(x => x.Orientation == Orientation.Vertical);
            verticalBarBar = verticalBar;

            if (verticalBarBar != null)
            {
                verticalBarBar.Scroll += verticalBarBar_Scroll;
            }
        }

        void verticalBarBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType != ScrollEventType.EndScroll)
            {
                return;
            }

            ScrollBar bar = sender as ScrollBar;
            if (bar == null)
            {
                return;
            }

            if (e.NewValue >= bar.Maximum)
            {
                lockToBottom = true;
            }
            else
            {
                lockToBottom = false;
            }
        }

        void ScrollToBottom()
        {
            var scrollViewer = chatView.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
            //if (!lockToBottom)
            //{
            //    return;
            //}

            //int selectedIndex = chatView.Items.Count - 1;
            //if (selectedIndex < 0)
            //{
            //    return;
            //}
            //chatView.ScrollIntoView(chatView.Items[selectedIndex]);
        }

        private async void chatBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await client.SendIRCMessage(chatBox.Text);
                chatBox.Text = "";
            }
        }

        private void chatButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatGrid.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                chatGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //chatButton.Label = "Open Chat";
            }
            else
            {
                chatGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //chatButton.Label = "Close Chat";
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
    }
}
