using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Tracing.Models.Annotations;

namespace Tracing.Models
{
    public class TracingDocument : INotifyPropertyChanged
    {
        private DocumentStatus _status;
        private DocumentType _type;
        private bool _isUnsaved;
        private string _title;
        private StorageFile _inkFile;

        public StorageFile ImageFile { get; set; }

        public StorageFile InkFile
        {
            get => _inkFile;
            set
            {
                _inkFile = value;
                Title = value.Name;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public DocumentStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                IsUnsaved = value == DocumentStatus.Modified;
                OnPropertyChanged();
            }
        }

        public DocumentType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public bool IsUnsaved
        {
            get => _isUnsaved;
            set { _isUnsaved = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum DocumentType
    {
        TempOrNew,
        Existing
    }
}
