using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SM.Media;
using SM.Media.Utility;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TwixelApp
{
    public delegate void StreamerObjectErrorHandler(object source, StreamerObjectErrorEventArgs e);
    public class StreamerObjectErrorEventArgs : EventArgs
    {
        public string ErrorString { get; internal set; }
    }

    public class StreamerObject
    {
        static readonly IApplicationInformation applicationInformation = ApplicationInformationFactory.DefaultTask.Result;
        IMediaStreamFacade mediaStreamFascade;
        MediaElement mediaElement;
        CoreDispatcher Dispatcher;
        Uri currentStreamUrl;
        public event StreamerObjectErrorHandler StreamerObjectErrorEvent;

        Action PlayPauseAction;

        public StreamerObject(CoreDispatcher Dispatcher, MediaElement mediaElement, Action playPauseAction)
        {
            this.Dispatcher = Dispatcher;
            this.mediaElement = mediaElement;
            this.PlayPauseAction = playPauseAction;
            // Need to call the Unloaded event in every page this Object is used and call
            // OnUnload for the Unloaded method event.
        }

        void UpdateTrack(SystemMediaTransportControls systemMediaTransportControls, string streamName, string streamDescription)
        {
            var displayUpdater = systemMediaTransportControls.DisplayUpdater;
            displayUpdater.ClearAll();
            displayUpdater.Type = MediaPlaybackType.Video;
            displayUpdater.VideoProperties.Title = streamName;
            displayUpdater.VideoProperties.Subtitle = streamDescription;
            displayUpdater.Update();
        }

        void UpdateThumbnail(SystemMediaTransportControls systemMediaTransportControls, Uri thumbnailUrl)
        {
            var displayUpdater = systemMediaTransportControls.DisplayUpdater;
            displayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(thumbnailUrl);
            displayUpdater.Update();
        }

        public void SetTrackTitle(string streamName, string streamDescription)
        {
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            UpdateTrack(smtc, streamName, streamDescription);
        }

        public void SetThumbnail(string thumbnailUrl)
        {
            if (thumbnailUrl == "" || thumbnailUrl == null)
            {
                return;
            }

            Uri thumbnail = new Uri(thumbnailUrl);
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            UpdateThumbnail(smtc, thumbnail);
        }

        async void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SystemControlsHandleButton(args.Button));
            }
            catch (Exception ex)
            {
                CreateError("System Media Button Pressed: " + args.Button + " Error Message: " + ex.Message);
            }
        }

        void SystemControlsHandleButton(SystemMediaTransportControlsButton button)
        {
            if (button == SystemMediaTransportControlsButton.Play)
            {
                PlayPauseAction.Invoke();
            }
            else if (button == SystemMediaTransportControlsButton.Pause)
            {
                PlayPauseAction.Invoke();
            }
            else if (button == SystemMediaTransportControlsButton.Stop)
            {
                PlayPauseAction.Invoke();
            }
        }

        public void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            var state = null == mediaElement ? MediaElementState.Closed : mediaElement.CurrentState;

            if (mediaElement.CurrentState == MediaElementState.Buffering)
            {
                Debug.WriteLine("---------- THE CURRENT MEDIA STATE IS BUFFERING -----------");
            }
            else if (mediaElement.CurrentState == MediaElementState.Opening)
            {
                Debug.WriteLine("---------- THE CURRENT MEDIA STATE IS OPENING -----------");
            }
            else if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                Debug.WriteLine("---------- THE CURRENT MEDIA STATE IS PLAYING -----------");
            }

            if (null != mediaStreamFascade)
            {
                var managerState = mediaStreamFascade.State;

                if (MediaElementState.Closed == state)
                {
                    if (TsMediaManager.MediaState.OpenMedia == managerState || TsMediaManager.MediaState.Opening == managerState || TsMediaManager.MediaState.Playing == managerState)
                    {
                        state = MediaElementState.Opening;
                    }
                }
            }

            UpdateState(state);
        }

        void UpdateState(MediaElementState state)
        {
            var smtc = SystemMediaTransportControls.GetForCurrentView();

            if (state == MediaElementState.Closed)
            {
                // play enabled, stop not enabled
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
            else if (state == MediaElementState.Playing)
            {
                // play not enabled, stop enabled
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (state == MediaElementState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (state == MediaElementState.Stopped)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
            }
        }

        public void SetStreamUrl(Uri streamUrl)
        {
            currentStreamUrl = streamUrl;
        }

        public void StartStream()
        {
            if (mediaElement == null)
            {
                // element is null
                return;
            }

            if (mediaElement.CurrentState == MediaElementState.Paused)
            {
                mediaElement.Play();
                return;
            }

            var task = PlayCurrentTrackAsync();
            TaskCollector.Default.Add(task, "MainPage.OnPlay() PlayCurrentTrackAsync");
        }

        async Task PlayCurrentTrackAsync()
        {
            if (mediaElement.Source != null)
            {
                mediaElement.Source = null;
            }

            try
            {
                InitializeMediaStream();

                var mss = await mediaStreamFascade.CreateMediaStreamSourceAsync(currentStreamUrl, CancellationToken.None);

                if (mss == null)
                {
                    CreateError("PlayCurrentTrackAsync() was unable to create a media stream source with no exception");
                    return;
                }
                mediaElement.SetMediaStreamSource(mss);
            }
            catch (Exception ex)
            {
                CreateError("PlayCurrentTrackAsync() was unable to create a media stream source with exception: " + ex.Message);
                return;
            }

            mediaElement.Play();
        }

        void InitializeMediaStream()
        {
            if (mediaStreamFascade != null)
            {
                return;
            }

            mediaStreamFascade = MediaStreamFacadeSettings.Parameters.Create();
            mediaStreamFascade.StateChange += TsMediaManagerOnStateChange;
        }

        // IDK if this is broken or not. However it should only be called by the MediaFailed method.
        public void CleanupMediaStream()
        {
            mediaElement.Source = null;

            if (mediaStreamFascade == null)
            {
                return;
            }

            mediaStreamFascade.StateChange -= TsMediaManagerOnStateChange;
            mediaStreamFascade.DisposeSafe();
            mediaStreamFascade = null;
        }

        void TsMediaManagerOnStateChange(object sender, TsMediaManagerStateEventArgs tsMediaManagerStateEventArgs)
        {
            var awaiter = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    var message = tsMediaManagerStateEventArgs.Message;

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        CreateError("TsMediaManagerOnStateChange error. Message: " + message);
                    }

                    mediaElement_CurrentStateChanged(null, null);
                });
        }

        public void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null)
            {
                return;
            }

            if (mediaElement.IsFullWindow && mediaElement.IsAudioOnly)
            {
                mediaElement.IsFullWindow = false;
            }
        }

        public void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("----- MEDIA FAILED ------");
            CreateError("Playing media failed. Error message: " + e.ErrorMessage);
            CleanupMediaStream();
            Stop();

            // enable play button
        }

        public void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("----- MEDIA ENDED ------");
            Stop();
        }

        public void Stop()
        {
            if (mediaElement != null)
            {
                mediaElement.Source = null;
            }
        }

        public void OnNavigatedFrom()
        {
            Stop();
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            smtc.ButtonPressed -= smtc_ButtonPressed;
        }

        public void OnNavigatedTo(string streamName, string streamDescription)
        {
            Stop();
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            UpdateTrack(smtc, streamName, streamDescription);
            smtc.ButtonPressed += smtc_ButtonPressed;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsNextEnabled = false;
            smtc.IsPreviousEnabled = false;
        }

        public void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            mediaElement_CurrentStateChanged(sender, e);
        }

        public void OnUnload()
        {
            Stop();
            var mediaStreamFasacde = mediaStreamFascade;
            this.mediaStreamFascade = null;
            mediaStreamFascade.DisposeBackground("MainPage unload");
        }

        void CreateError(string errorStr)
        {
            StreamerObjectErrorEventArgs error = new StreamerObjectErrorEventArgs();
            error.ErrorString = errorStr;
            StreamerObjectErrorEvent(this, error);
        }
    }
}
