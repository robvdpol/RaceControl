using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;

namespace RaceControl.Core.Mvvm
{
    public class ExtendedDialogService : DialogService, IExtendedDialogService
    {
        public ExtendedDialogService(IContainerExtension containerExtension) : base(containerExtension)
        {
        }

        public bool SelectFolder(string title, string initialDirectory, out string folder)
        {
            folder = null;

            var dialog = new CommonOpenFileDialog
            {
                Title = title,
                DefaultDirectory = initialDirectory,
                InitialDirectory = initialDirectory,
                IsFolderPicker = true,
                Multiselect = false,
                AllowNonFileSystemItems = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folder = dialog.FileName;

                return true;
            }

            return false;
        }

        public bool SelectFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename)
        {
            filename = null;

            var dialog = new CommonSaveFileDialog
            {
                Title = title,
                DefaultDirectory = initialDirectory,
                InitialDirectory = initialDirectory,
                DefaultFileName = initialFilename,
                DefaultExtension = defaultExtension,
                AlwaysAppendDefaultExtension = true,
                OverwritePrompt = true,
                IsExpandedMode = true
            };

            dialog.Filters.Add(new CommonFileDialogFilter($"{defaultExtension}-files", $"*{defaultExtension}"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                filename = dialog.FileName;

                return true;
            }

            return false;
        }

        public object Show(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner, string windowName = null)
        {
            return ShowDialogInternal(name, parameters, callback, hasOwner, windowName);
        }

        private object ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult> callback, bool hasOwner, string windowName)
        {
            var dialogWindow = CreateDialogWindow(windowName);
            ConfigureDialogWindowEvents(dialogWindow, callback);
            ConfigureDialogWindowContent(name, dialogWindow, parameters);

            if (!hasOwner && dialogWindow.Owner != null)
            {
                dialogWindow.Owner = null;
            }

            dialogWindow.Show();

            return dialogWindow.DataContext;
        }
    }
}