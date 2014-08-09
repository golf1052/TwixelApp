using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TwixelAPI;

namespace TwixelApp
{
    public class SubscriberEmote
    {
        public string channelName;
        public List<Emote> emotes;
        public WebUrl badge;
        public long set;

        public SubscriberEmote(string channelName,
            JObject emotes,
            string badgeUrl,
            long set)
        {
            this.channelName = channelName;
            this.emotes = LoadEmotes(emotes);
            this.badge = new WebUrl(badgeUrl);
            this.set = set;
        }

        List<Emote> LoadEmotes(JObject o)
        {
            List<Emote> emotes = new List<Emote>();
            foreach (KeyValuePair<string, JToken> emote in o)
            {
                emotes.Add(new Emote((string)emote.Key, (string)emote.Value, null));
            }
            return emotes;
        }
    }
}
