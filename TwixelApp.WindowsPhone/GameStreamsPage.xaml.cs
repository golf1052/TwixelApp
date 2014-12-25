using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameStreamsPage : Page
    {
        ObservableCollection<GameStreamsGridViewBinding> streamsCollection;
        bool loadedStream = false;

        public GameStreamsPage()
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
            List<object> parameters = (List<object>)e.Parameter;
            Game game = (Game)parameters[0];
            BitmapImage gameImage = new BitmapImage(game.logoMedium.url);
            gameLogo.Source = gameImage;
            gameName.Text = game.name;
            gameChannels.Text = game.channels.ToString();
            gameViewers.Text = game.viewers.ToString();

            streamsCollection = new ObservableCollection<GameStreamsGridViewBinding>();
            List<TwixelAPI.Stream> streams = new List<TwixelAPI.Stream>();
            streams = await AppConstants.twixel.RetrieveStreams(game.name, new List<string>(), 100, false, false);
            foreach (TwixelAPI.Stream stream in streams)
            {
                streamsCollection.Add(new GameStreamsGridViewBinding(stream));
            }

            do
            {
                // Fix implemented so that streams
                // stop loading after you navigate away from the page
                if (loadedStream)
                {
                    break;
                }

                streams = await AppConstants.twixel.RetrieveStreams(true);
                foreach (TwixelAPI.Stream stream in streams)
                {
                    if (loadedStream)
                    {
                        break;
                    }

                    streamsCollection.Add(new GameStreamsGridViewBinding(stream));
                }

                if (loadedStream)
                {
                    break;
                }
            }
            while (streams.Count != 0);
        }

        private void gameStreamsView_Loaded(object sender, RoutedEventArgs e)
        {
            gameStreamsView.ItemsSource = streamsCollection;
        }

        private async void gameStreamsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            loadedStream = true;
            GameStreamsGridViewBinding streamItem = (GameStreamsGridViewBinding)e.ClickedItem;

            List<object> parameters = new List<object>();
            parameters.Add(streamItem.stream);
            Dictionary<AppConstants.Quality, Uri> qualities = await AppConstants.GetQualities(streamItem.stream.channel.name);
            parameters.Add(qualities);
            Frame.Navigate(typeof(StreamPage), parameters);
        }

        private async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = e.NewSize.Width;
            double height = e.NewSize.Height;

            if (width < height)
            {
                // Portrait
                await StatusBar.GetForCurrentView().ShowAsync();
            }
            else
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }
        }
    }
}
