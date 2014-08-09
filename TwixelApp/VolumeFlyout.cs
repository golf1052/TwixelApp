using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TwixelApp
{
    /// <summary>
    /// Helps control a volume slider and a mute button
    /// </summary>
    public class VolumeFlyout
    {
        int muteVolume;
        bool isMuted;
        bool wasJustMuted;
        Slider volumeSlider;
        Button muteButton;
        AppBarButton volumeButton;
        MediaElement streamPlayer;

        public VolumeFlyout(Slider volumeSlider,
            Button muteButton,
            AppBarButton volumeButton,
            MediaElement streamPlayer)
        {
            this.muteVolume = 100;
            this.isMuted = false;
            this.wasJustMuted = false;
            this.volumeButton = volumeButton;
            this.volumeSlider = volumeSlider;
            this.muteButton = muteButton;
            this.streamPlayer = streamPlayer;
            volumeSlider.ValueChanged += volumeSlider_ValueChanged;
            muteButton.Click += muteButton_Click;
        }

        void muteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!isMuted)
            {
                wasJustMuted = true;
                isMuted = true;
                streamPlayer.Volume = 0;
                volumeSlider.Value = 0;
                volumeButton.Label = volumeSlider.Value.ToString();
                ((SymbolIcon)muteButton.Content).Symbol = Symbol.Mute;
            }
            else
            {
                isMuted = false;
                streamPlayer.Volume = muteVolume / 100;
                volumeButton.Label = muteVolume.ToString();
                volumeSlider.Value = muteVolume;
                ((SymbolIcon)muteButton.Content).Symbol = Symbol.Volume;
            }
        }

        void volumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!wasJustMuted)
            {
                if (isMuted)
                {
                    isMuted = false;
                    ((SymbolIcon)muteButton.Content).Symbol = Symbol.Volume;
                }
                streamPlayer.Volume = volumeSlider.Value / 100;
                muteVolume = (int)volumeSlider.Value;
                volumeButton.Label = volumeSlider.Value.ToString();
            }
            wasJustMuted = false;
        }
    }
}
