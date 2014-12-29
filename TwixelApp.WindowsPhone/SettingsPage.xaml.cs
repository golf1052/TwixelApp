using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TwixelApp.Constants;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TwixelApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            wifiComboBox.SelectedIndex = (int)AppConstants.wifiQuality;
            cellularComboBox.SelectedIndex = (int)AppConstants.cellularQuality;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SaveSettings();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        async void SaveSettings()
        {
            StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile wifiQualityFile = await AppConstants.TryGetFile("wifiFile.json", roamingFolder);
            StorageFile cellularQualityFile = await AppConstants.TryGetFile("cellularQualityFile", roamingFolder);
            System.Diagnostics.Debug.WriteLine(((ComboBoxItem)wifiComboBox.SelectedItem).Content.ToString());
            await AppConstants.OverwriteFile(wifiQualityFile, ((ComboBoxItem)wifiComboBox.SelectedItem).Content.ToString());
            await AppConstants.OverwriteFile(cellularQualityFile, ((ComboBoxItem)cellularComboBox.SelectedItem).Content.ToString());
        }
    }
}
