using System.Collections.Generic;

namespace TwixelApp
{
    public class TwitchChatUser
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<int> EmoteSet { get; set; }

        public TwitchChatUser(string name)
        {
            this.Name = name;
        }
    }
}
