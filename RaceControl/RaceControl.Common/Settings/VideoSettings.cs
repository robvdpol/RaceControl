using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace RaceControl.Common.Settings
{
    public class VideoSettings : BindableBase, IVideoSettings
    {
        private const string FILENAME = "settings.json";

        private readonly ILogger _logger;

        private bool _lowQualityMode;
        private bool _useAlternativeStream;
        private bool _enableRecording;
        private string _recordingLocation = Environment.CurrentDirectory;

        public VideoSettings(ILogger logger)
        {
            _logger = logger;
        }

        public bool LowQualityMode
        {
            get => _lowQualityMode;
            set => SetProperty(ref _lowQualityMode, value);
        }

        public bool UseAlternativeStream
        {
            get => _useAlternativeStream;
            set => SetProperty(ref _useAlternativeStream, value);
        }

        public bool EnableRecording
        {
            get => _enableRecording;
            set => SetProperty(ref _enableRecording, value);
        }

        public string RecordingLocation
        {
            get => _recordingLocation;
            set => SetProperty(ref _recordingLocation, value);
        }

        public void Load()
        {
            _logger.Info("Loading video settings...");

            if (!File.Exists(FILENAME))
            {
                return;
            }

            try
            {
                using (var file = File.OpenText(FILENAME))
                {
                    new JsonSerializer().Populate(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while loading video settings.");
            }

            _logger.Info("Done loading video settings.");
        }

        public void Save()
        {
            _logger.Info("Saving video settings...");

            try
            {
                using (var file = File.CreateText(FILENAME))
                {
                    new JsonSerializer().Serialize(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while saving video settings.");
            }

            _logger.Info("Done saving video settings.");
        }
    }
}