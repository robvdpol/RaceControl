using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public interface IExtendedDialogService : IDialogService
    {
        void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner);
    }
}