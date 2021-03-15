using Prism.Services.Dialogs;

namespace RaceControl.Extensions
{
    public interface IExtendedDialogService : IDialogService
    {
        bool SelectFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename);
    }
}