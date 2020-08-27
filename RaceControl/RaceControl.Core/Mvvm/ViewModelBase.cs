using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        protected readonly ILogger Logger;

        private ICommand _previewKeyDownCommand;
        private ICommand _keyDownCommand;
        private bool _isBusy;

        protected ViewModelBase(ILogger logger)
        {
            Logger = logger;
        }

        public ICommand PreviewKeyDownCommand => _previewKeyDownCommand ??= new DelegateCommand<KeyEventArgs>(PreviewKeyDownExecute);
        public ICommand KeyDownCommand => _keyDownCommand ??= new DelegateCommand<KeyEventArgs>(KeyDownExecute);

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        protected void CleanupProcess(Process process)
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"An error occurred while trying to kill process with id '{process.Id}' and name '{process.ProcessName}'.");
                    }
                }

                process.Dispose();
            }
        }

        private void PreviewKeyDownExecute(KeyEventArgs args)
        {
            if (IsBusy)
            {
                args.Handled = true;
            }
        }

        private void KeyDownExecute(KeyEventArgs args)
        {
            if (IsBusy)
            {
                args.Handled = true;
            }
        }
    }
}