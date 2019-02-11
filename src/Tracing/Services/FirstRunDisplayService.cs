using System;
using System.Threading.Tasks;
using Tracing.Configuration;
using Tracing.Core;
using Tracing.Core.Infrastructure;

namespace Tracing.Services
{
    public class FirstRunDisplayService
    {
        public static async Task ShowIfAppropriateAsync(Action firstRunAction)
        {
            bool hasShownFirstRun = false;
            hasShownFirstRun = await Windows.Storage.ApplicationData.Current.LocalSettings.ReadAsync<bool>(nameof(hasShownFirstRun));

            if (!hasShownFirstRun)
            {
                await Windows.Storage.ApplicationData.Current.LocalSettings.SaveAsync(nameof(hasShownFirstRun), true);
                firstRunAction();
            }
        }
    }
}
