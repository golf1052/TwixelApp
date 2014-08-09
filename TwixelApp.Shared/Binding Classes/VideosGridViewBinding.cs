using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;

namespace TwixelApp
{
    public class VideosGridViewBinding
    {
        public Uri Image { get; set; }
        public int Viewers { get; set; }
        public string Title { get; set; }
        public string Length { get; set; }
        public WebUrl Url { get; set; }

        public VideosGridViewBinding(string image, int viewers, string title, TimeSpan length, string url)
        {
            this.Image = new Uri(image);
            this.Viewers = viewers;
            this.Title = title;
            this.Length = length.Minutes.ToString() + ":" + length.Seconds.ToString();
            this.Url = new WebUrl(url);
        }

        public VideosGridViewBinding(Video video)
        {
            if (video.preview != null)
            {
                this.Image = video.preview.url;
            }
            this.Viewers = video.views;
            this.Title = video.title;
            TimeSpan length = TimeSpan.FromSeconds(video.length);
            if (length.Seconds > 10)
            {
                this.Length = length.Minutes.ToString() + ":" + length.Seconds.ToString();
            }
            else
            {
                this.Length = length.Minutes.ToString() + ":0" + length.Seconds.ToString();
            }
            this.Url = video.url;
        }
    }
}
