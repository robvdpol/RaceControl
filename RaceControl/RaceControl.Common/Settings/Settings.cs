using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace RaceControl.Common.Settings
{
    public class Settings : BindableBase, ISettings
    {
        private const string Filename = "RaceControl.settings.json";

        private readonly ILogger _logger;
        private readonly JsonSerializer _serializer;

        private bool _lowQualityMode;
        private bool _useAlternativeStream;
        private string _recordingLocation = Environment.CurrentDirectory;
        private string _latestRelease;
        private ObservableCollection<string> _selectedSeries;

        public Settings(ILogger logger, JsonSerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
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

        public string RecordingLocation
        {
            get => _recordingLocation;
            set => SetProperty(ref _recordingLocation, value);
        }

        public string LatestRelease
        {
            get => _latestRelease;
            set => SetProperty(ref _latestRelease, value);
        }

        public ObservableCollection<string> SelectedSeries
        {
            get => _selectedSeries ??= new ObservableCollection<string>();
            set => SetProperty(ref _selectedSeries, value);
        }

        public void Load()
        {
            _logger.Info("Loading settings...");

            if (!File.Exists(Filename))
            {
                return;
            }

            try
            {
                using (var file = File.OpenText(Filename))
                {
                    _serializer.Populate(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while loading settings.");
            }

            _logger.Info("Done loading settings.");
        }

        public void Save()
        {
            _logger.Info("Saving settings...");

            try
            {
                using (var file = File.CreateText(Filename))
                {
                    _serializer.Serialize(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while saving settings.");
            }

            _logger.Info("Done saving settings.");
        }
    }
}