namespace RaceControl.Common.Settings
{
    public interface IVideoSettings
    {
        bool LowQualityMode { get; set; }

        bool UseAlternativeStream { get; set; }

        bool EnableRecording { get; set; }

        string RecordingLocation { get; set; }
    }
}