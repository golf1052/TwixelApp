using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TwixelAPI;
using TwixelAPI.Constants;
using TwixelApp.Constants;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FinalConfirmation : Page
    {
        List<TwitchConstants.Scope> scopes;

        public FinalConfirmation()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> param = (List<object>)e.Parameter;
            scopes = (List<TwitchConstants.Scope>)param[0];
            Uri webPage = AppConstants.twixel.Login(scopes);
            twitchLoginWebView.Navigate(webPage);

            if (!scopes.Contains(TwitchConstants.Scope.UserRead))
            {
                userReadBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.UserBlocksEdit))
            {
                userBlocksEditBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.UserBlocksRead))
            {
                userBlocksReadBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.UserFollowsEdit))
            {
                userFollowsEditBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelRead))
            {
                channelReadBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelEditor))
            {
                channelEditorBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelCommercial))
            {
                channelCommercialBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelStream))
            {
                channelStreamBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelSubscriptions))
            {
                channelStreamBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.UserSubcriptions))
            {
                userSubscriptionsBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChannelCheckSubscription))
            {
                channelCheckSubscriptionBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (!scopes.Contains(TwitchConstants.Scope.ChatLogin))
            {
                chatLoginBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async void twitchLoginWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (twitchLoginWebView.Source.Host == "golf1052.com")
            {
                if (twitchLoginWebView.Source.Query != "?error=access_denied&error_description=The+user+denied+you+access")
                {
                    twitchLoginWebView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    string[] splitString = twitchLoginWebView.Source.Fragment.Split('=');
                    string[] secondSplitString = splitString[1].Split('&');
                    string[] scopes = splitString[2].Split('+');
                    List<TwitchConstants.Scope> authorizedScopes = new List<TwitchConstants.Scope>();
                    foreach (string scope in scopes)
                    {
                        authorizedScopes.Add(TwitchConstants.StringToScope(scope));
                    }
                    User user = null;
                    user = await AppConstants.twixel.RetrieveUserWithAccessToken(secondSplitString[0]);
                    JObject userData = new JObject();
                    userData["active"] = 0;
                    JObject userO = new JObject();
                    userO["name"] = user.name;
                    userO["access_token"] = user.accessToken;
                    userData["user"] = userO;
                    StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
                    StorageFile usersFile = await roamingFolder.CreateFileAsync("usersFile.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(usersFile, userData.ToString());
                    AppConstants.ActiveUser = user;
                    //Frame.Navigate(typeof(HomePage));
                }
                else if (twitchLoginWebView.Source.Query == "?error=access_denied&error_description=The+user+denied+you+access")
                {
                    //Frame.Navigate(typeof(HomePage));
                }
            }
        }
    }
}
