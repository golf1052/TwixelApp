using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace TwixelApp
{
    public sealed partial class AboutFlyout : SettingsFlyout
    {
        public AboutFlyout()
        {
            this.InitializeComponent();
        }

        private async void emailButton_Click(object sender, RoutedEventArgs e)
        {
            LauncherOptions options = new LauncherOptions();
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;
            await Launcher.LaunchUriAsync(new Uri("mailto:golf1052@live.com"), options);
        }
    }
}
