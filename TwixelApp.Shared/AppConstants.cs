using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwixelAPI;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using TwixelApp;
using Windows.UI.Popups;

namespace TwixelApp.Constants
{
    public static class AppConstants
    {
        public static User ActiveUser { get; set; }
        public static List<Emote> emotes = new List<Emote>();
        public static List<SubscriberEmote> subscriberEmotes = new List<SubscriberEmote>();
        public static Dictionary<long, string> sets = new Dictionary<long, string>();

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
                secondResponse = await GetWebData(new Uri("http://usher.twitch.tv/select/" + channel + ".json?allow_source=true&nauthsig=" + sig + "&nauth=" + token + "&type=any"));
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
            bool startedStream = false;
            q = AppConstants.Quality.Source;

            foreach (KeyValuePair<AppConstants.Quality, Uri> quality in qualities)
            {
                if (quality.Key == q)
                {
                    streamerObject.StartStream(quality.Value);
                    startedStream = true;
                    break;
                }
            }

            if (!startedStream)
            {
                foreach (KeyValuePair<AppConstants.Quality, Uri> quality in qualities)
                {
                    if (quality.Key == AppConstants.Quality.Chunked)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                }
            }

            if (!startedStream)
            {
                foreach (KeyValuePair<AppConstants.Quality, Uri> quality in qualities)
                {
                    if (quality.Key == AppConstants.Quality.Source)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                    if (quality.Key == AppConstants.Quality.High)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                    else if (quality.Key == AppConstants.Quality.Medium)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                    else if (quality.Key == AppConstants.Quality.Low)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                    else if (quality.Key == AppConstants.Quality.Mobile)
                    {
                        streamerObject.StartStream(quality.Value);
                        startedStream = true;
                        break;
                    }
                }
            }
        }
    }
}
