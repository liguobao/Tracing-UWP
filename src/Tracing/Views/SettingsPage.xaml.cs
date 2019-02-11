using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Tracing.Controls;
using Tracing.Helpers;

namespace Tracing.Views
{
    public sealed partial class SettingsPage : Page
    {
        private CoreApplicationViewTitleBar _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public SettingsPage()
        {
            InitializeComponent();

            _coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(GrdTitle);

            if (!RadialController.IsSupported())
            {
                ToggleVibrate.IsEnabled = false;
                BtnWheelSettings.IsEnabled = false;
            }
        }

        private async void ShowWhatsNew(object sender, TappedRoutedEventArgs e)
        {
            await ShowUpdateHistory();
        }

        private async void BtnRestart_OnClick(object sender, RoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private async void LnkUpdateHistory_OnClick(object sender, RoutedEventArgs e)
        {
            await ShowUpdateHistory();
        }

        private async Task ShowUpdateHistory()
        {
            var text = await Helper.LoadDocument("Assets/update-history/Update.md");
            MdText.Text = text;
            MdText.TextWrapping = TextWrapping.Wrap;
            await DigUpdateHistory.ShowAsync();
        }
    }
}
