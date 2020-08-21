using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public interface IExtendedDialogService : IDialogService
    {
        bool SelectFolder(string title, string initialDirectory, out string folder);

        bool SelectFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename);

        void Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner);
    }
}