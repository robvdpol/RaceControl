using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm
{
    public abstract class DialogViewModelBase : ViewModelBase, IDialogAware
    {
        private ICommand _closeWindowCommand;

        private string _title;

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute);

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public event Action<IDialogResult> RequestClose;

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
        }

        protected virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private void CloseWindowExecute()
        {
            RaiseRequestClose(null);
        }
    }
}