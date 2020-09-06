using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface ISettings
    {
        bool LowQualityMode { get; set; }

        bool UseAlternativeStream { get; set; }

        bool DisableStreamlink { get; set; }

        string RecordingLocation { get; set; }

        string LatestRelease { get; set; }

        ObservableCollection<string> SelectedSeries { get; set; }

        void Load();

        void Save();
    }
}