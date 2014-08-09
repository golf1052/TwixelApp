using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;

namespace TwixelApp
{
    public class GameStreamsGridViewBinding
    {
        public string Name { get; set; }
        public int Viewers { get; set; }
        public Uri Image { get; set; }
        public string Description { get; set; }

        public Stream stream;

        public GameStreamsGridViewBinding(string name, int viewers, Uri image, string description)
        {
            this.Name = name;
            this.Viewers = viewers;
            this.Image = image;
            this.Description = description;
        }

        public GameStreamsGridViewBinding(Stream stream)
        {
            this.Name = stream.channel.displayName;
            this.Viewers = (int)stream.viewers;
            this.Image = stream.preview.url;
            this.Description = stream.channel.status;
            this.stream = stream;
        }
    }
}
