using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RaceControl.Views;
using System.Windows;

namespace RaceControl.Extensions
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
                AllowNonFileSystemItems = false,
                EnsurePathExists = true
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

        protected override void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
        {
            base.ConfigureDialogWindowProperties(window, dialogContent, viewModel);

            if (window is VideoDialogWindow)
            {
                window.Owner = null;
            }
        }
    }
}