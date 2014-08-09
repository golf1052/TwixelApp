using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace TwixelApp
{
    public class ChatListViewBinding
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public FontWeight FontWeight { get; set; }
        public ObservableCollection<UIElement> ChatThings { get; set; }

        public ChatListViewBinding(string name, string message, bool boldName)
        {
            this.Name = name;
            this.Message = message;
            if (boldName)
            {
                FontWeight = FontWeights.Bold;
            }
            else
            {
                FontWeight = FontWeights.Normal;
            }
        }

        public ChatListViewBinding(string name, ObservableCollection<UIElement> elements, bool boldName)
        {
            this.Name = name;
            this.ChatThings = elements;
            if (boldName)
            {
                FontWeight = FontWeights.Bold;
            }
            else
            {
                FontWeight = FontWeights.Normal;
            }
        }
    }
}
