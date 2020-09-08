using NLog;
using Prism.Mvvm;
using RaceControl.Core.Helpers;
using System;
using System.Windows;

namespace RaceControl.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        protected readonly ILogger Logger;

        private bool _isBusy;

        protected ViewModelBase(ILogger logger)
        {
            Logger = logger;
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        protected void HandleNonFatalError(Exception ex)
        {
            Logger.Error(ex, "A non-fatal error occurred.");
        }

        protected void HandleFatalError(Exception ex)
        {
            Logger.Error(ex, "A fatal error occurred.");
            Application.Current.Dispatcher.Invoke(() => MessageBoxHelper.ShowError(ex.Message));
            NotBusyAnymore();
        }

        protected void NotBusyAnymore()
        {
            IsBusy = false;
        }
    }
}