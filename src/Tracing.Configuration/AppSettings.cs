using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Tracing.Configuration
{
    public class AppSettings : INotifyPropertyChanged
    {
        public bool UsePrimaryLanguageOverride
        {
            get => ReadSettings(nameof(UsePrimaryLanguageOverride), false);
            set
            {
                SaveSettings(nameof(UsePrimaryLanguageOverride), value);
                NotifyPropertyChanged();
            }
        }

        public string PrimaryLanguageOverride
        {
            get => ReadSettings(nameof(PrimaryLanguageOverride), "en-US");
            set
            {
                SaveSettings(nameof(PrimaryLanguageOverride), value);
                NotifyPropertyChanged();
            }
        }



        public int MaxCanvasHeight => DefaultCanvasHeight * 2;

        public int DefaultCanvasHeight
        {
            get => ReadSettings(nameof(DefaultCanvasHeight), 900);
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                SaveSettings(nameof(DefaultCanvasHeight), value);
                NotifyPropertyChanged();
            }
        }

        public int DefaultCanvasWidth
        {
            get => ReadSettings(nameof(DefaultCanvasWidth), 1440);
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                SaveSettings(nameof(DefaultCanvasWidth), value);
                NotifyPropertyChanged();
            }
        }

        public bool EnableSurfaceDial
        {
            get => ReadSettings(nameof(EnableSurfaceDial), true);
            set
            {
                SaveSettings(nameof(EnableSurfaceDial), value);
                NotifyPropertyChanged();
            }
        }

        public bool IsPressureEnabled
        {
            get => ReadSettings(nameof(IsPressureEnabled), true);
            set
            {
                SaveSettings(nameof(IsPressureEnabled), value);
                NotifyPropertyChanged();
            }
        }

        public bool EnableVibrateForSurfaceDial
        {
            get => ReadSettings(nameof(EnableVibrateForSurfaceDial), true);
            set
            {
                SaveSettings(nameof(EnableVibrateForSurfaceDial), value);
                NotifyPropertyChanged();
            }
        }

        public bool IsAutoSaveEnabled
        {
            get => ReadSettings(nameof(IsAutoSaveEnabled), false);
            set
            {
                SaveSettings(nameof(IsAutoSaveEnabled), value);
                NotifyPropertyChanged();
            }
        }

        public bool ShowEraseAllMenuButton
        {
            get => ReadSettings(nameof(ShowEraseAllMenuButton), true);
            set
            {
                SaveSettings(nameof(ShowEraseAllMenuButton), value);
                NotifyPropertyChanged();
            }
        }

        public bool ShowProtractor
        {
            get => ReadSettings(nameof(ShowProtractor), true);
            set
            {
                SaveSettings(nameof(ShowProtractor), value);
                NotifyPropertyChanged();
            }
        }

        public bool FitToCurve
        {
            get => ReadSettings(nameof(FitToCurve), true);
            set
            {
                SaveSettings(nameof(FitToCurve), value);
                NotifyPropertyChanged();
            }
        }

        public bool EnablePenTilt
        {
            get => ReadSettings(nameof(EnablePenTilt), true);
            set
            {
                SaveSettings(nameof(EnablePenTilt), value);
                NotifyPropertyChanged();
            }
        }

        public bool ShowShapeRecognitionButton
        {
            get => ReadSettings(nameof(ShowShapeRecognitionButton), true);
            set
            {
                SaveSettings(nameof(ShowShapeRecognitionButton), value);
                NotifyPropertyChanged();
            }
        }

        public ApplicationDataContainer SettingsContainer { get; set; }

        public AppSettings()
        {
            SettingsContainer = ApplicationData.Current.LocalSettings;
        }

        private void SaveSettings(string key, object value)
        {
            SettingsContainer.Values[key] = value;
        }

        private T ReadSettings<T>(string key, T defaultValue)
        {
            if (SettingsContainer.Values.ContainsKey(key))
            {
                return (T)SettingsContainer.Values[key];
            }
            if (null != defaultValue)
            {
                return defaultValue;
            }
            return default(T);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
