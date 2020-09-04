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

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute, CanCloseDialog).ObservesProperty(() => CanClose);

        public abstract string Title { get; }

        public bool CanClose
        {
            get => _canClose;
            protected set => SetProperty(ref _canClose, value);
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
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

        public void CloseWindow(bool force = false)
        {
            if (force)
            {
                CanClose = true;
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