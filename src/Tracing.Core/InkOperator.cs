using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Edi.UWP.Helpers;
using Microsoft.Graphics.Canvas;

namespace Tracing.Core
{
    public class InkOperator
    {
        public InkCanvas InkCanvas { get; }

        public StorageFile CurrentTargetImageFile { get; private set; }

        public StorageFile CurrentInkFile { get; private set; }

        public InkToShapeAssKicker InkToShapeAssKicker { get; set; }

        public InkOperator(InkCanvas inkCanvas)
        {
            InkCanvas = inkCanvas;
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

            InkToShapeAssKicker = new InkToShapeAssKicker(inkCanvas);

            UndoStrokes = new Stack<InkStroke>();
            InkCanvas.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
            InkCanvas.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
        }

        private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            InkToShapeAssKicker.CollectStrokes(args.Strokes);
        }

        public void ApplyImageFile(StorageFile sFile)
        {
            CurrentTargetImageFile = sFile;
        }

        public async Task ApplyInkFile(StorageFile sFile)
        {
            CurrentInkFile = sFile;
            if (sFile != null)
            {
                var file = await sFile.OpenReadAsync();
                if (file.Size > 0)
                {
                    await InkCanvas.InkPresenter.StrokeContainer.LoadAsync(file);
                }
            }
        }

        #region Undo / Redo

        public Stack<InkStroke> UndoStrokes { get; }

        public void UndoLastStorke()
        {
            IReadOnlyList<InkStroke> strokes = InkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {
                strokes[strokes.Count - 1].Selected = true;
                UndoStrokes.Push(strokes[strokes.Count - 1]);
                InkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }
        }

        public void RedoLastStorke()
        {
            if (UndoStrokes.Count > 0)
            {
                var stroke = UndoStrokes.Pop();

                // This will blow up sky high:
                // InkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);

                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = stroke.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
                InkStroke stk = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
                InkCanvas.InkPresenter.StrokeContainer.AddStroke(stk);
            }
        }

        private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            foreach (var stroke in args.Strokes.OrderByDescending(s => s.Id))
            {
                UndoStrokes.Push(stroke);
            }

            InkToShapeAssKicker.EraseStrokes(args.Strokes);
        }

        #endregion

        #region Save to Image

        public async Task<WriteableBitmap> SaveInkImageToBitmap()
        {
            StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("Tracing_Temp.png", CreationCollisionOption.ReplaceExisting);
            var tempExportFile = await SaveInkImageToStorageFile(tempFile, Colors.Transparent);
            var stream = await tempExportFile.OpenReadAsync();
            var wb = new WriteableBitmap(1, 1);
            wb.SetSource(stream);
            return wb;
        }

        public async Task<StorageFile> SaveInkImageToStorageFile(StorageFile file, Color backgroundColor)
        {
            CachedFileManager.DeferUpdates(file);

            CanvasDevice device = CanvasDevice.GetSharedDevice();

            var localDpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)InkCanvas.ActualWidth, (int)InkCanvas.ActualHeight, localDpi);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(backgroundColor);
                ds.DrawInk(InkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
            }

            await CachedFileManager.CompleteUpdatesAsync(file);
            return file;
        }

        public async Task<FileUpdateStatus> ExportInkToImageFile(PickerLocationId location, string fileName, Color backgroundColor)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = location
            };
            savePicker.FileTypeChoices.Add("Png Image", new[] { ".png" });
            savePicker.SuggestedFileName = fileName;
            StorageFile sFile = await savePicker.PickSaveFileAsync();
            if (sFile != null)
            {
                CachedFileManager.DeferUpdates(sFile);
                CanvasDevice device = CanvasDevice.GetSharedDevice();

                var localDpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
                CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)InkCanvas.ActualWidth, (int)InkCanvas.ActualHeight, localDpi);

                using (var ds = renderTarget.CreateDrawingSession())
                {
                    ds.Clear(backgroundColor);
                    ds.DrawInk(InkCanvas.InkPresenter.StrokeContainer.GetStrokes());
                }

                using (var fileStream = await sFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
                }

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(sFile);
                return status;
            }
            return FileUpdateStatus.Failed;
        }

        #endregion

        #region Save to Ink File / ISF-GIF

        public async Task<Response<StorageFile>> SaveInkToInkFile(PickerLocationId location)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = location
            };
            picker.FileTypeChoices.Add("INK files", new List<string> { ".ink" });
            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return new Response<StorageFile>
                {
                    IsSuccess = false,
                    Message = $"{nameof(file)} is null",
                    Item = null
                };
            }
            var response = await Utils.SaveToStorageFile(InkCanvas, file);
            return new Response<StorageFile>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Item = file
            };
        }

        public async Task<bool> SaveInkToGif(PickerLocationId location)
        {
            IReadOnlyList<InkStroke> currentStrokes = InkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Strokes present on ink canvas.
            if (currentStrokes.Count > 0)
            {
                FileSavePicker savePicker =
                    new FileSavePicker { SuggestedStartLocation = location };
                savePicker.FileTypeChoices.Add(
                    "GIF with embedded ISF",
                    new List<string> { ".gif" });
                savePicker.DefaultFileExtension = ".gif";
                savePicker.SuggestedFileName = "Tracing-ISF-Gif";

                // Show the file picker.
                StorageFile file = await savePicker.PickSaveFileAsync();

                // When chosen, picker returns a reference to the selected file.
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
                    {
                        await InkCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                        await outputStream.FlushAsync();
                    }
                    stream.Dispose();

                    FileUpdateStatus status =
                        await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == FileUpdateStatus.Complete)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        public async Task<bool> SaveInkToStorageFile(StorageFile file = null)
        {
            if (null == file)
            {
                file = CurrentInkFile;
            }
            var response = await Utils.SaveToStorageFile(InkCanvas, file);
            return response.IsSuccess;
        }

        #endregion
    }
}
