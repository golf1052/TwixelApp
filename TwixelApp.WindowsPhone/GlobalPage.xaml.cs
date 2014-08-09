using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using TwixelAPI;
using TwixelApp.Constants;
using Windows.Storage;
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
        Twixel twixel;

        public GlobalPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            twixel = (Twixel)e.Parameter;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            //await localFolder.DeleteAsync();
            loadingText.Text = "looking for users data";
            try
            {
                StorageFile usersFile = await localFolder.GetFileAsync("usersFile.json");
                string usersData = await FileIO.ReadTextAsync(usersFile);
                loadingText.Text = "users data found";
                Dictionary<string, string> users = new Dictionary<string, string>();
                loadingText.Text = "reading users data";
                JObject usersO = JObject.Parse(usersData);
                JArray usersArray = (JArray)usersO["users"];
                foreach (JObject user in usersArray)
                {
                    users.Add((string)user["name"], (string)user["access_token"]);
                }

                foreach (KeyValuePair<string, string> user in users)
                {
                    loadingText.Text = "loading " + user.Key.ToString();
                    User tempUser = await twixel.CreateUserWithAccessToken(user.Value);
                    if (tempUser == null)
                    {
                        loadingText.Text = user.Value + "'s token invalid";
                        Frame.Navigate(typeof(FirstLaunchInfo), twixel);
                        return;
                    }
                }
                AppConstants.ActiveUser = twixel.users[(int)usersO["active"]];
                Frame.Navigate(typeof(HomePage), twixel);
            }
            catch (FileNotFoundException ex)
            {
                loadingText.Text = "users data not found";
                Frame.Navigate(typeof(FirstLaunchInfo), twixel);
            }
        }
    }
}
