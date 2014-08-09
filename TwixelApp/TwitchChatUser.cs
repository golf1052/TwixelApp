using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
