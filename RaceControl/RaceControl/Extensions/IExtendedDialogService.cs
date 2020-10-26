using Prism.Services.Dialogs;

namespace RaceControl.Extensions
{
    public interface IExtendedDialogService : IDialogService
    {
        bool SelectFolder(string title, string initialDirectory, out string folder);

        bool SelectFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename);
    }
}