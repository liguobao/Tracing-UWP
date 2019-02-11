using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI;
using Windows.UI.Notifications;
using Edi.UWP.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Tracing.Core.Infrastructure;
using Tracing.Services;

namespace Tracing.Helpers
{
    public class Helper
    {
        public static Size GetCanvasSizeFromScreenResolution()
        {
            var w = UI.GetScreenWidth();
            var h = UI.GetScreenHeight();
            var dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var ns = new Size(w / dpi - 100, h / dpi - 200);
            return ns;
        }

        public static async Task<bool> IsCameraPresent()
        {
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            return devices.Count > 0;
        }

        public static async Task<string> LoadDocument(string relativePath, int howManyLines = 0, bool stopAtEmptyLine = false)
        {
            try
            {
                var storageFile = await Package.Current.InstalledLocation.GetFileAsync(relativePath.Replace('/', '\\'));
                using (var stream = await storageFile.OpenReadAsync())
                {
                    var sm = stream.AsStream();

                    string str = string.Empty;
                    using (var reader = new StreamReader(sm))
                    {
                        if (howManyLines > 0 && !stopAtEmptyLine)
                        {
                            for (int i = 0; i < howManyLines; i++)
                            {
                                str += reader.ReadLine() + Environment.NewLine;
                            }
                        }
                        else if (howManyLines == 0 && stopAtEmptyLine)
                        {
                            string content;
                            while (null != (content = reader.ReadLine()) && !string.IsNullOrWhiteSpace(content))
                            {
                                str += content + Environment.NewLine;
                            }
                        }
                        else
                        {
                            str = reader.ReadToEnd();
                        }
                    }

                    return str;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error Reading {relativePath}", e.Message);
                throw;
            }
        }

        public static void SetTitlebarAccentColor(bool isMainUIColor = true)
        {
            var accentColor = Colors.Transparent;
            //if (isMainUIColor)
            //{
            //    accentColor = Color.FromArgb(255, 69, 69, 69);
            //}

            var btnHoverColor = Color.FromArgb(128,
                (byte)(accentColor.R + 30),
                (byte)(accentColor.G + 30),
                (byte)(accentColor.B + 30));

            UI.ApplyColorToTitleBar(
                accentColor,
                Colors.White,
                btnHoverColor,
                Colors.LightGray);

            UI.ApplyColorToTitleButton(
                accentColor, Colors.White,
                btnHoverColor, Colors.White,
                accentColor, Colors.White,
                btnHoverColor, Colors.LightGray);
        }

        public static async Task SendFeedback(string msg, bool includeDeviceInfo)
        {
            var deviceInfo = new EasClientDeviceInformation();

            string subject = "Image Portray (Tracing / 描图) Feedback";
            string body = $"Message: {msg}\n\n" +
                          $"App Version: {Utils.GetAppVersion()} \n";

            if (includeDeviceInfo)
            {
                body += $"Device Name: {deviceInfo.FriendlyName}, " +
                        $"OS Version: {deviceInfo.OperatingSystem}, " +
                        $"SKU: {deviceInfo.SystemSku}, " +
                        $"Product: {deviceInfo.SystemProductName}, " +
                        $"Manufacturer: {deviceInfo.SystemManufacturer}, " +
                        $"Firmware Version: {deviceInfo.SystemFirmwareVersion}, " +
                        $"Hardware Version: {deviceInfo.SystemHardwareVersion}）";
            }
            else
            {
                body += ")";
            }

            string to = "Edi.Wang@outlook.com";
            await Tasks.OpenEmailComposeAsync(to, subject, body);
        }
    }
}
