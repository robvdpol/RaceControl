using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public interface IExtendedDialogService : IDialogService
    {
        bool SelectFolder(string title, string initialDirectory, out string folder);

        void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner);
    }
}