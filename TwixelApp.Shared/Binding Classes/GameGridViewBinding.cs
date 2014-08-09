using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;

namespace TwixelApp
{
    public class GameGridViewBinding
    {
        public string Name { get; set; }
        public int Viewers { get; set; }
        public int Channels { get; set; }
        public Uri Image { get; set; }

        public Game game;

        public GameGridViewBinding(string name, int viewers, int channels, Uri image)
        {
            this.Name = name;
            this.Viewers = viewers;
            this.Channels = channels;
            this.Image = image;
        }

        public GameGridViewBinding(Game game)
        {
            this.Name = game.name;
            this.Viewers = (int)game.viewers;
            this.Channels = (int)game.channels;
            this.Image = game.boxLarge.url;
            this.game = game;
        }
    }
}
