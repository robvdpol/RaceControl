namespace RaceControl.Extensions;

public interface IExtendedDialogService : IDialogService
{
    bool SaveFile(string title, string initialDirectory, string initialFilename, string defaultExtension, out string filename);

    bool OpenFile(string title, string initialDirectory, string defaultExtension, out string filename);
}