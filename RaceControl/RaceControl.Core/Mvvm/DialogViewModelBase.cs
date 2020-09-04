using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm
{
    public abstract class DialogViewModelBase : ViewModelBase, IExtendedDialogAware
    {
        private ICommand _closeWindowCommand;
        private bool _initialized;

        protected DialogViewModelBase(ILogger logger) : base(logger)
        {
        }

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute, CanCloseDialog).ObservesProperty(() => Initialized);

        public abstract string Title { get; }

        protected bool Initialized
        {
            get => _initialized;
            private set => SetProperty(ref _initialized, value);
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return Initialized;
        }

        public virtual void OnDialogClosed()
        {
            Initialized = false;
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            Initialized = true;
        }

        public void CloseWindow(bool force = false)
        {
            if (force)
            {
                Initialized = true;
            }

            if (CloseWindowCommand.CanExecute(null))
            {
                CloseWindowCommand.Execute(null);
            }
        }

        protected virtual void CloseWindowExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.None));
        }

        protected void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
    }
}