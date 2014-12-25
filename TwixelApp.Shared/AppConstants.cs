using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwixelAPI;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using TwixelApp;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Networking.Connectivity;

namespace TwixelApp.Constants
{
    public static class AppConstants
    {
        public static Twixel twixel;
        public static User ActiveUser { get; set; }
        public static List<Emote> emotes = new List<Emote>();
        public static List<SubscriberEmote> subscriberEmotes = new List<SubscriberEmote>();
        public static Dictionary<long, string> sets = new Dictionary<long, string>();
        public static Quality wifiQuality;
        public static Quality cellularQuality;
        public static Quality preferredQuality;

        public enum Quality
        {
            Source,
            High,
            Medium,
            Low,
            Mobile,
            Chunked
        }

        public static async void ShowError(string message)
        {
            MessageDialog messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        public static async Task<string> GetWebData(Uri uri)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // 200 - OK
                string responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            else
            {
                return "Unknown status code";
            }
        }

        /// <summary>
        /// Tries to create a file. If the file is there it just returns it.
        /// If it is not there it creates the file and returns it.
        /// </summary>
        /// <param name="fileName">Desired file name</param>
        /// <param name="folder">StorageFolder the file goes in</param>
        /// <returns>A file</returns>
        public static async Task<StorageFile> TryGetFile(string fileName, StorageFolder folder)
        {
            StorageFile file;
#if WINDOWS_PHONE_APP
            file = await AppConstants.TryGetItemAsync(folder, fileName);
#else
            file = await folder.TryGetItemAsync(fileName) as StorageFile;
#endif

            if (file != null)
            {
                return file;
            }
            else
            {
                file = await folder.CreateFileAsync(fileName);
                return file;
            }
        }

        public static async Task OverwriteFile(StorageFile file, string data)
        {
            await FileIO.WriteTextAsync(file, data);
        }

        public static async Task<StorageFile> TryGetItemAsync(StorageFolder folder, string fileName)
        {
            StorageFile file;

            try
            {
                file = await folder.GetItemAsync(fileName) as StorageFile;
                return file;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Dictionary<AppConstants.Quality, Uri>> GetQualities(string channel)
        {
            string response;
            do
            {
                response = await GetWebData(new Uri("http://api.twitch.tv/api/channels/" + channel + "/access_token"));
            }
            while (response == "Unknown status code");

            JObject accessToken = JObject.Parse(response);
            string token = (string)accessToken["token"];
            string sig = (string)accessToken["sig"];
            string secondResponse;
            do
            {
                secondResponse = await GetWebData(new Uri("http://usher.twitch.tv/api/channel/hls/" + channel + ".m3u8?token=" + token + "&sig=" + sig + "&allow_source=true"));
            }
            while (secondResponse == "Unknown status code");

            if (secondResponse == "[]")
            {
                // stream is offline
                return null;
            }
            else
            {
                string[] lines = secondResponse.Split('\n');
                Dictionary<AppConstants.Quality, Uri> qualities = new Dictionary<AppConstants.Quality, Uri>();
                foreach (string line in lines)
                {
                    if (line.Contains("/source/"))
                    {
                        qualities.Add(AppConstants.Quality.Source, new Uri(line));
                    }
                    else if (line.Contains("/high/"))
                    {
                        qualities.Add(AppConstants.Quality.High, new Uri(line));
                    }
                    else if (line.Contains("/medium/"))
                    {
                        qualities.Add(AppConstants.Quality.Medium, new Uri(line));
                    }
                    else if (line.Contains("/low/"))
                    {
                        qualities.Add(AppConstants.Quality.Low, new Uri(line));
                    }
                    else if (line.Contains("/mobile/"))
                    {
                        qualities.Add(AppConstants.Quality.Mobile, new Uri(line));
                    }
                    else if (line.Contains("/chunked/"))
                    {
                        qualities.Add(AppConstants.Quality.Chunked, new Uri(line));
                    }
                }

                return qualities;
            }
        }

        public static void PlayPreferredQuality(Dictionary<AppConstants.Quality, Uri> qualities, AppConstants.Quality q, StreamerObject streamerObject)
        {
            if (q == Quality.Source)
            {
                if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
                else if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
            }
            else if (q == Quality.High)
            {
                if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
                else if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
            }
            else if (q == Quality.Medium)
            {
                if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
                else if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
            }
            else if (q == Quality.Low)
            {
                if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
                else if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
            }
            else if (q == Quality.Mobile)
            {
                if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
                else if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
            }
            else if (q == Quality.Chunked)
            {
                if (qualities.ContainsKey(Quality.Chunked))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Chunked]);
                }
                else if (qualities.ContainsKey(Quality.Mobile))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Mobile]);
                }
                else if (qualities.ContainsKey(Quality.Low))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Low]);
                }
                else if (qualities.ContainsKey(Quality.Medium))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Medium]);
                }
                else if (qualities.ContainsKey(Quality.High))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.High]);
                }
                else if (qualities.ContainsKey(Quality.Source))
                {
                    streamerObject.SetStreamUrl(qualities[Quality.Source]);
                }
            }

            streamerObject.StartStream();
        }

        public static bool OnWifi()
        {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile.IsWlanConnectionProfile)
            {
                return true;
            }
            else if (connectionProfile.IsWwanConnectionProfile)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        public static void DeterminePreferredQuality()
        {
            if (OnWifi())
            {
                preferredQuality = wifiQuality;
            }
            else
            {
                preferredQuality = cellularQuality;
            }
        }
    }
}
