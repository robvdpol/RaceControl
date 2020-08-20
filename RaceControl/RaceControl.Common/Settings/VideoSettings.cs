using System;
using System.IO;

namespace RaceControl.Common.Settings
{
    public class VideoSettings : IVideoSettings
    {
        public VideoSettings()
        {
            RecordingLocation = Path.Combine(Environment.CurrentDirectory, "Recordings");
        }

        public bool LowQualityMode { get; set; }

        public bool UseAlternativeStream { get; set; }

        public bool EnableRecording { get; set; }

        public string RecordingLocation { get; set; }
    }
}