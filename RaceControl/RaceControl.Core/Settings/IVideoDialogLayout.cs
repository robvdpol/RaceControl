using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface IVideoDialogLayout
    {
        ObservableCollection<VideoDialogSettings> Instances { get; }

        void Load();

        bool Save();
    }
}