using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TwixelAPI;
using TwixelApp.Constants;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GlobalPageWP : Page
    {
        public GlobalPageWP()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AppConstants.twixel.TwixelErrorEvent += twixel_TwixelErrorEvent;
        }

        void twixel_TwixelErrorEvent(object source, TwixelErrorEventArgs e)
        {

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AppConstants.twixel.TwixelErrorEvent -= twixel_TwixelErrorEvent;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            loadingText.Text = "checking internet connection";
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            NetworkConnectivityLevel level = connectionProfile.GetNetworkConnectivityLevel();

            if (level != NetworkConnectivityLevel.InternetAccess)
            {
                MessageDialog message = new MessageDialog("You are not connected to the internet. Check your connection setings then restart the app.");
                await message.ShowAsync();
                loadingText.Text = "no internet connection found";
                return;
            }

            loadingText.Text = "loading twitch emotes";
            string globalString = await AppConstants.GetWebData(new Uri("http://www.twitchemotes.com/global.json"));
            JObject globalEmotes = JObject.Parse(globalString);
            foreach (KeyValuePair<string, JToken> o in globalEmotes)
            {
                AppConstants.emotes.Add(new Emote((string)o.Key, (string)o.Value["url"], (string)o.Value["description"]));
            }

            string subscriberString = await AppConstants.GetWebData(new Uri("http://www.twitchemotes.com/subscriber.json"));
            JObject subscriberEmotes = JObject.Parse(subscriberString);
            foreach (KeyValuePair<string, JToken> o in subscriberEmotes)
            {
                AppConstants.subscriberEmotes.Add(new SubscriberEmote((string)o.Key, (JObject)o.Value["emotes"], (string)o.Value["_badge"], (long)o.Value["_set"]));
            }

            string setString = await AppConstants.GetWebData(new Uri("http://twitchemotes.com/api/sets"));
            JObject setMap = JObject.Parse(setString);
            foreach (KeyValuePair<string, JToken> o in setMap)
            {
                AppConstants.sets.Add(long.Parse(o.Key), (string)o.Value);
            }

            StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
            loadingText.Text = "looking for settings";
            await LoadQualitySettings(roamingFolder);
            loadingText.Text = "looking for users data";
            await LoadUserData(roamingFolder);
        }

        async Task LoadUserData(StorageFolder roamingFolder)
        {
            try
            {
                StorageFile usersFile = await roamingFolder.GetFileAsync("usersFile.json");
                string usersData = await FileIO.ReadTextAsync(usersFile);
                loadingText.Text = "users data found";
                Dictionary<string, string> users = new Dictionary<string, string>();
                loadingText.Text = "reading users data";
                JObject usersO = JObject.Parse(usersData);
                JObject userO = (JObject)usersO["user"];
                users.Add((string)userO["name"], (string)userO["access_token"]);
                //JArray usersArray = (JArray)usersO["users"];
                //foreach (JObject user in usersArray)
                //{
                //    users.Add((string)user["name"], (string)user["access_token"]);
                //}

                User tempUser = null;
                foreach (KeyValuePair<string, string> user in users)
                {
                    loadingText.Text = "loading " + user.Key.ToString();
                    tempUser = await AppConstants.twixel.RetrieveUserWithAccessToken(user.Value);
                    if (tempUser == null)
                    {
                        loadingText.Text = user.Value + "'s token invalid";
                        Frame.Navigate(typeof(FirstLaunchInfo));
                        return;
                    }
                }
                AppConstants.ActiveUser = tempUser;
                Frame.Navigate(typeof(HomePage));
            }
            catch (FileNotFoundException ex)
            {
                loadingText.Text = "users data not found";
                Frame.Navigate(typeof(FirstLaunchInfo));
            }
        }

        async Task LoadQualitySettings(StorageFolder roamingFolder)
        {
            StorageFile wifiQualityFile = await AppConstants.TryGetFile("wifiFile.json", roamingFolder);
            string wifiQuality = await FileIO.ReadTextAsync(wifiQualityFile);
            if (wifiQuality == "")
            {
                wifiQuality = "source";
                AppConstants.wifiQuality = AppConstants.Quality.Source;
                await AppConstants.OverwriteFile(wifiQualityFile, wifiQuality);
            }
            else
            {
                if (wifiQuality == "source")
                {
                    AppConstants.wifiQuality = AppConstants.Quality.Source;
                }
                else if (wifiQuality == "high")
                {
                    AppConstants.wifiQuality = AppConstants.Quality.High;
                }
                else if (wifiQuality == "medium")
                {
                    AppConstants.wifiQuality = AppConstants.Quality.Medium;
                }
                else if (wifiQuality == "low")
                {
                    AppConstants.wifiQuality = AppConstants.Quality.Low;
                }
                else if (wifiQuality == "mobile")
                {
                    AppConstants.wifiQuality = AppConstants.Quality.Mobile;
                }
                else
                {
                    AppConstants.wifiQuality = AppConstants.Quality.Source;
                }
            }

            StorageFile cellularQualityFile = await AppConstants.TryGetFile("cellularFile.json", roamingFolder);
            string cellularQuality = await FileIO.ReadTextAsync(cellularQualityFile);
            if (cellularQuality == "")
            {
                cellularQuality = "mobile";
                AppConstants.cellularQuality = AppConstants.Quality.Mobile;
                await AppConstants.OverwriteFile(cellularQualityFile, cellularQuality);
            }
            else
            {
                if (cellularQuality == "source")
                {
                    AppConstants.cellularQuality = AppConstants.Quality.Source;
                }
                else if (cellularQuality == "high")
                {
                    AppConstants.cellularQuality = AppConstants.Quality.High;
                }
                else if (cellularQuality == "medium")
                {
                    AppConstants.cellularQuality = AppConstants.Quality.Medium;
                }
                else if (cellularQuality == "low")
                {
                    AppConstants.cellularQuality = AppConstants.Quality.Low;
                }
                else if (cellularQuality == "mobile")
                {
                    AppConstants.cellularQuality = AppConstants.Quality.Mobile;
                }
                else
                {
                    AppConstants.cellularQuality = AppConstants.Quality.Mobile;
                }
            }
        }
    }
}
