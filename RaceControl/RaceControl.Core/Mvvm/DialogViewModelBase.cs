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
        private bool _canClose;

        protected DialogViewModelBase(ILogger logger) : base(logger)
        {
        }

        public abstract string Title { get; }

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute, CanCloseDialog).ObservesProperty(() => CanClose);

        protected bool CanClose
        {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        public event Action<IDialogResult> RequestClose;

        public virtual bool CanCloseDialog()
        {
            return CanClose;
        }

        public virtual void OnDialogClosed()
        {
            CanClose = false;
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            CanClose = true;
        }

        public void CloseWindow(bool forceClose = false)
        {
            if (forceClose)
            {
                CanClose = true;
            }

            CloseWindowExecute();
        }

        protected void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private void CloseWindowExecute()
        {
            RaiseRequestClose(null);
        }
    }
}