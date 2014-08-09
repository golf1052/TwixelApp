using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;

namespace TwixelApp
{
    public class Emote
    {
        public string name;
        public WebUrl url;
        public string description;

        public Emote(string name,
            string url,
            string description)
        {
            this.name = name;
            this.url = new WebUrl(url);
            this.description = description;
        }
    }
}
