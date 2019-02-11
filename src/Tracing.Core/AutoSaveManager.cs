using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Tracing.Core
{
    public class AutoSaveManager
    {
        public string TempInkFileName { get; }

        public StorageFile TempInkStorageFile { get; private set; }

        private InkOperator InkOperator { get; }

        public DispatcherTimer DispatcherTimer { get; set; }

        public AutoSaveManager(InkOperator inkOperator)
        {
            InkOperator = inkOperator;
            TempInkFileName = "Tracing_Temp.ink";

            DispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
            DispatcherTimer.Tick += DispatcherTimerOnTick;
        }

        private async void DispatcherTimerOnTick(object sender, object o)
        {
            await SaveTempSession();
        }

        public void Resume()
        {
            DispatcherTimer.Start();
        }

        public void Suspend()
        {
            DispatcherTimer.Stop();
        }

        public async Task<StorageFile> InitializeTempInkFile()
        {
            var tempInkFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(TempInkFileName, CreationCollisionOption.OpenIfExists);
            TempInkStorageFile = tempInkFile;
            await InkOperator.ApplyInkFile(tempInkFile);
            return TempInkStorageFile;
        }

        private async Task DestroyTempInkFile()
        {
            await TempInkStorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async Task SaveTempSession()
        {
            await InkOperator.SaveInkToStorageFile(TempInkStorageFile);
        }

        public async Task LoadLastSession()
        {
            await InkOperator.ApplyInkFile(TempInkStorageFile);
        }
    }
}
