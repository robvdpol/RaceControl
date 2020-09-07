using Prism.Services.Dialogs;

namespace RaceControl.Core.Mvvm
{
    public interface IExtendedDialogAware : IDialogAware
    {
        void CloseWindow(bool forceClose = false);
    }
}