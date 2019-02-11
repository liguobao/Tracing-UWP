using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Tracing.Services;
using System.Linq;
using CommonServiceLocator;
using Tracing.ViewModels;
using Tracing.Views;

namespace Tracing.Activation
{
    public class FileLaunchActivationHandler : ActivationHandler<FileActivatedEventArgs>
    {
        private readonly string _navElement;

        private NavigationServiceEx NavigationService => ServiceLocator.Current.GetInstance<NavigationServiceEx>();

        public FileLaunchActivationHandler()
        {
            _navElement = typeof(MainViewModel).FullName; // navElement.FullName;
        }

        protected override async Task HandleInternalAsync(FileActivatedEventArgs args)
        {
            NavigationService.Navigate(_navElement, args);

            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(FileActivatedEventArgs args)
        {
            return args.Files.Any();
            //return NavigationService.Frame.Content == null;
        }
    }
}
