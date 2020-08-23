using System.Collections.ObjectModel;

namespace RaceControl.Common.Settings
{
    public interface ISettings
    {
        bool LowQualityMode { get; set; }

        bool UseAlternativeStream { get; set; }

        bool EnableRecording { get; set; }

        string RecordingLocation { get; set; }

        ObservableCollection<string> SelectedSeries { get; set; }

        void Load();

        void Save();
    }
}