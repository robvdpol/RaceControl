namespace RaceControl.Common.Settings
{
    public class VideoSettings : IVideoSettings
    {
        public bool LowQualityMode { get; set; }

        public bool UseAlternativeStream { get; set; }

        public bool EnableRecording { get; set; }
    }
}