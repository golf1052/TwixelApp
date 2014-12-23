using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif
using TwixelAPI;
using TwixelApp.Constants;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace TwixelApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        Twixel twixel = new Twixel(ApiKey.clientID, ApiKey.clientSecret, "http://golf1052.com");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

#if WINDOWS_APP
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
        }

        void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            //args.Request.ApplicationCommands.Add(new SettingsCommand("About", "About", (handler) => ShowAboutFlyout()));
            //args.Request.ApplicationCommands.Add(new SettingsCommand("Accounts", "Accounts", (handler) => ShowAccountsFlyout()));
            args.Request.ApplicationCommands.Add(new SettingsCommand("Privacy Policy", "Privacy Policy", (handler) => ShowPrivacyPolicyFlyout()));
        }

        public void ShowAboutFlyout()
        {
            AboutFlyout aboutFlyout = new AboutFlyout();
            aboutFlyout.Show();
        }

        public void ShowAccountsFlyout()
        {
            AccountsFlyout accountsFlyout = new AccountsFlyout((Frame)Window.Current.Content);
            accountsFlyout.Show();
        }

        public void ShowPrivacyPolicyFlyout()
        {
            PrivacyPolicyFlyout privacyPolicyFlyout = new PrivacyPolicyFlyout();
            privacyPolicyFlyout.Show();
        }
#endif

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            AppConstants.twixel = twixel;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
#if WINDOWS_PHONE_APP
                rootFrame.Navigate(typeof(GlobalPageWP), twixel);
#else
                rootFrame.Navigate(typeof(GlobalPage));
#endif

            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
