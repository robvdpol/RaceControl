namespace RaceControl.Core.Settings;

public interface IVideoDialogLayout
{
    ObservableCollection<VideoDialogSettings> Instances { get; }

    bool Load(string filename = null);

    bool Save();

    bool Import(string filename);
}