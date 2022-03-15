using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Ioc;

namespace RaceControl.Extensions;

public class ExtendedDialogService : DialogService, IExtendedDialogService
{
    public ExtendedDialogService(IContainerExtension containerExtension) : base(containerExtension)
    {
    }

    public bool SaveFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename)
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

    public bool OpenFile(string title, string initialDirectory, string defaultExtension, out string filename)
    {
        filename = null;

        var dialog = new CommonOpenFileDialog
        {
            Title = title,
            DefaultDirectory = initialDirectory,
            InitialDirectory = initialDirectory,
            DefaultExtension = defaultExtension,
            EnsureFileExists = true,
            EnsurePathExists = true
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