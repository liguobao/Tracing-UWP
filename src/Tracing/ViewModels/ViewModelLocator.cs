using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Tracing.Services;

namespace Tracing.ViewModels
{
    public class ViewModelLocator
    {
        NavigationServiceEx _navigationService = new NavigationServiceEx();

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(() => _navigationService);
            SimpleIoc.Default.Register<IDialogService, DialogService>();

            Register<WelcomeFlipViewModel, Views.WelcomeFlipPage>();
            Register<MainViewModel, Views.MainPage>();
            Register<SettingsViewModel, Views.SettingsPage>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public WelcomeFlipViewModel WelcomeFlipViewModel => ServiceLocator.Current.GetInstance<WelcomeFlipViewModel>();

        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public void Register<TVm, TV>() where TVm : class
        {
            SimpleIoc.Default.Register<TVm>();
            _navigationService.Configure(typeof(TVm).FullName, typeof(TV));
        }
    }
}