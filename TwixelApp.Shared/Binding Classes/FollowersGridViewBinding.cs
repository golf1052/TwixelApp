using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;

namespace TwixelApp
{
    public class FollowersGridViewBinding
    {
        public Uri ProfileImage { get; set; }
        public string Name { get; set; }
        public Channel Channel { get; set; }

        public FollowersGridViewBinding(string profileImage, string name)
        {
            this.Name = name;
            if (profileImage != null)
            {
                this.ProfileImage = new Uri(profileImage);
            }
            else
            {
                this.ProfileImage = new Uri("ms-appx:///Assets/defaultFollowerPicture.png");
            }
        }

        public FollowersGridViewBinding(User user)
        {
            this.Name = user.displayName;
            if (user.logo != null)
            {
                this.ProfileImage = user.logo.url;
            }
            else
            {
                this.ProfileImage = new Uri("ms-appx:///Assets/defaultFollowerPicture.png");
            }
        }
    }
}
