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
        private bool _opened;

        public ICommand CloseWindowCommand => _closeWindowCommand ??= new DelegateCommand(CloseWindowExecute, CanCloseDialog).ObservesProperty(() => Opened);

        public string Title
        {
            get => _title;
            protected set => SetProperty(ref _title, value);
        }

        public bool Opened
        {
            get => _opened;
            private set => SetProperty(ref _opened, value);
        }

        public event Action<IDialogResult> RequestClose;

        public virtual bool CanCloseDialog()
        {
            return Opened;
        }

        public virtual void OnDialogClosed()
        {
            Opened = false;
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            Opened = true;
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