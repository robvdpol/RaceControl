using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm
{
    public abstract class DialogViewModelBase : ViewModelBase, IDialogAware
    {
        private ICommand _closeWindowCommand;

        private bool _opened;
        private string _title;

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute);

        public string Title
        {
            get => _title;
            protected set => SetProperty(ref _title, value);
        }

        public event Action<IDialogResult> RequestClose;

        public virtual bool CanCloseDialog()
        {
            return _opened;
        }

        public virtual void OnDialogClosed()
        {
            _opened = false;
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            _opened = true;
        }

        protected void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private void CloseWindowExecute()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.None));
        }
    }
}