using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using CommonServiceLocator;
using Tracing.Services;

namespace Tracing.Activation
{
    public class DefaultLaunchActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        private readonly string _navElement;
    
        private NavigationServiceEx NavigationService => ServiceLocator.Current.GetInstance<NavigationServiceEx>();

        public DefaultLaunchActivationHandler(Type navElement)
        {
            _navElement = navElement.FullName;
        }
    
        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate(_navElement, args.Arguments);
            
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return NavigationService.Frame.Content == null;
        }
    }
}
