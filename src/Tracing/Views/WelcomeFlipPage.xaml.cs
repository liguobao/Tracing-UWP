using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using Tracing.Helpers;

namespace Tracing.Views
{
    public sealed partial class WelcomeFlipPage : Page
    {
        private CoreApplicationViewTitleBar _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public string Publisher => Edi.UWP.Helpers.Utils.GetAppPublisher();

        public string Version => $"{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";

        public string OperatingSystemVersion => $"OS Build: {SystemInformation.OperatingSystemVersion}";

        public string CPUArch => SystemInformation.OperatingSystemArchitecture.ToString();

        public WelcomeFlipPage()
        {
            this.InitializeComponent();

            Helper.SetTitlebarAccentColor(false);
            _coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(GrdTitle);
        }

        private void GoToMainPage(object sender, TappedRoutedEventArgs e)
        {
            Helper.SetTitlebarAccentColor();
            Frame.Navigate(typeof(MainPage));
        }

        private void MainFlip_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            Helper.SetTitlebarAccentColor();
            Frame.Navigate(typeof(MainPage));
        }
    }
}
