using System;
using System.Collections.Generic;
using System.IO;
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
    public sealed partial class GlobalPage : Page
    {
        public GlobalPage()
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
            //await localFolder.DeleteAsync();
            loadingText.Text = "looking for users data";
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
    }
}
