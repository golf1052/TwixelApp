using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TwixelApp
{
    public class SearchFlyout
    {
        TextBox searchBox;
        Button searchButton;
        ComboBox searchOptions;
        Frame Frame;

        public SearchFlyout(TextBox searchBox,
            Button searchButton,
            ComboBox searchOptions,
            Frame Frame)
        {
            this.searchBox = searchBox;
            this.searchButton = searchButton;
            this.searchOptions = searchOptions;
            this.Frame = Frame;
            searchButton.Click += searchButton_Click;
        }

        void searchButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            List<object> parameters = new List<object>();
            parameters.Add(searchBox.Text);
            if (searchOptions.SelectedIndex == 0)
            {
                Frame.Navigate(typeof(SearchStreamsPage), parameters);
            }
            else if (searchOptions.SelectedIndex == 1)
            {
                Frame.Navigate(typeof(SearchGamesPage), parameters);
            }
        }
    }
}
