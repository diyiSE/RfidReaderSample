using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace unitechRFIDSample.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected bool _disposed;
        private DelegateCommand _loadedCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand LoadedCommand => _loadedCommand ??= new DelegateCommand(OnLoaded);

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnLoaded()
        {
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
}
