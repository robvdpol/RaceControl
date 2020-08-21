namespace RaceControl.Common.Settings
{
    public interface ISettings
    {
        bool LowQualityMode { get; set; }

        bool UseAlternativeStream { get; set; }

        bool EnableRecording { get; set; }

        string RecordingLocation { get; set; }

        void Load();

        void Save();
    }
}