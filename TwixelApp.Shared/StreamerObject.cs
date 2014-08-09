using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using SM.Media;
using SM.Media.Utility;
using SM.Media.Web;
using System.Net.Http.Headers;

namespace TwixelApp
{
    public class StreamerObject
    {
        static readonly TimeSpan stepSize = TimeSpan.FromMinutes(2);
        static readonly IApplicationInformation applicationInformation = ApplicationInformationFactory.DefaultTask.Result;
        readonly IHttpClients httpClients;
        readonly IMediaElementManager mediaElementManager;
        readonly DispatcherTimer positionSampler;
        IMediaStreamFascade mediaStreamFascade;
        TimeSpan previousPosition;
        MediaElement mediaElement;
        CoreDispatcher Dispatcher;

        public StreamerObject(CoreDispatcher Dispatcher, MediaElement mediaElement)
        {
            this.Dispatcher = Dispatcher;
            this.mediaElement = mediaElement;
            mediaElementManager = new WinRtMediaElementManager(Dispatcher, () =>
            {
                UpdateState(MediaElementState.Opening);
                return this.mediaElement;
            },
            me => UpdateState(MediaElementState.Closed));

#if WINDOWS_PHONE_APP
            var userAgent = CreateUserAgent();
#else
            var userAgent = applicationInformation.CreateUserAgent();
#endif
            httpClients = new HttpClients(userAgent: userAgent);
            positionSampler = new DispatcherTimer{Interval = TimeSpan.FromMilliseconds(75)};
            positionSampler.Tick += positionSampler_Tick;
            //Unloaded += (sender, args) => OnUnload();
        }

        public static ProductInfoHeaderValue CreateUserAgent()
        {
            var userAgent = HttpSettings.Parameters.UserAgentFactory("Twixel" ?? "Unknown", "Twixel" ?? "0.0");

            return userAgent;
        }

        public void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            var state = null == mediaElement ? MediaElementState.Closed : mediaElement.CurrentState;

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
            if (state == MediaElementState.Closed)
            {
                // play enabled, stop not enabled
            }
            else if (state == MediaElementState.Paused)
            {
                // play enabled, stop enabled
            }
            else if (state == MediaElementState.Playing)
            {
                // play not enabled, stop enabled
            }
            else
            {
                // stop enabled
            }
        }

        public void StartStream(Uri streamUrl)
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

            InitializeMediaStream();

            mediaStreamFascade.Source = streamUrl;

            mediaElement.Play();
        }

        void InitializeMediaStream()
        {
            if (mediaStreamFascade != null)
            {
                return;
            }

            mediaStreamFascade = MediaStreamFascadeSettings.Parameters.Create(httpClients, mediaElementManager.SetSourceAsync);
            mediaStreamFascade.SetParameter(mediaElementManager);
            mediaStreamFascade.StateChange += TsMediaManagerOnStateChange;
        }

        // This seems very broken right now...
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
                        // error message goes here
                    }

                    mediaElement_CurrentStateChanged(null, null);
                });
        }

        public void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("----- MEDIA FAILED ------");
            // e.ErrorMessage;

            //CleanupMediaStream();
            Stop();

            // enable play button
        }

        public void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("----- MEDIA ENDED ------");
            StopMedia();
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
            //CleanupMediaStream();
            Stop();
        }

        public void OnNavigatedTo()
        {
            //CleanupMediaStream();
            Stop();
        }

        void StopMedia()
        {
            if (mediaElement != null)
            {
                mediaElement.Source = null;
            }
        }

        void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            mediaElement_CurrentStateChanged(sender, e);
        }

        public void OnUnload()
        {
            if (mediaElement != null)
            {
                mediaElement.Source = null;
            }

            var mediaStreamFasacde = mediaStreamFascade;
            this.mediaStreamFascade = null;
            mediaStreamFascade.DisposeBackground("MainPage unload");
        }

        void positionSampler_Tick(object sender, object e)
        {
            throw new NotImplementedException();
        }
    }
}
