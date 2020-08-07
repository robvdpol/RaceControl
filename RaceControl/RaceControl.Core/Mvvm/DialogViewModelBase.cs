using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public abstract class DialogViewModelBase : BindableBase, IDialogAware
    {
        public abstract string Title { get; set; }

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
    }
}