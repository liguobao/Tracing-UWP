using System;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Tracing.Configuration;
using Tracing.Core;
using Tracing.Services;
using Tracing.ViewModels;
using UnhandledExceptionEventArgs = Windows.UI.Xaml.UnhandledExceptionEventArgs;

namespace Tracing
{
    sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        private ActivationService ActivationService => _activationService.Value;

        public static AppSettings AppSettings { get; set; }

        public App()
        {
            InitializeComponent();

            AppSettings = new AppSettings();

            UnhandledException += OnUnhandledException;
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            // TODO: Save App Data
            var message = e.Message;
            await new MessageDialog("We are sorry, but something just went very very wrong, trying to save your work. " +
                                    "\n\nError: " + message,
                                    "🙈 App Blow Up Sky High").ShowAsync();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            ApplicationLanguages.PrimaryLanguageOverride =
                AppSettings.UsePrimaryLanguageOverride ?
                    AppSettings.PrimaryLanguageOverride :
                    Windows.System.UserProfile.GlobalizationPreferences.Languages[0];

            if (!e.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(e);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(MainViewModel));
        }
    }
}
