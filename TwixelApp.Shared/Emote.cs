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
            if (url.StartsWith("http:"))
            {
                this.url = new WebUrl(url);
            }
            else
            {
                url = "http:" + url;
                this.url = new WebUrl(url);
            }
            this.description = description;
        }
    }
}
