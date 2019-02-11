using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace Tracing.Helpers
{
    public class WindowsShareHelper
    {
        private DataTransferManager Dtm { get; }

        public string Title { get; set; }

        public string Description { get; set; }

        public StorageFile InkImageFile { get; set; }

        public WindowsShareHelper(string title, string description)
        {
            Dtm = DataTransferManager.GetForCurrentView();
            Dtm.DataRequested += DtmOnDataRequested;

            Title = title;
            Description = description;
        }

        private async void DtmOnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            try
            {
                var requestData = args.Request.Data;
                requestData.Properties.Title = Title;
                requestData.Properties.Description = Description;

                var imageItems = new List<IStorageItem> { InkImageFile };
                requestData.SetStorageItems(imageItems);

                var imageStreamRef = RandomAccessStreamReference.CreateFromFile(InkImageFile);
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, Edi.UWP.Helpers.Utils.GetResource("Resources/BlowUp")).ShowAsync();
            }
        }

        public void Share()
        {
            DataTransferManager.ShowShareUI();
        }
    }
}
