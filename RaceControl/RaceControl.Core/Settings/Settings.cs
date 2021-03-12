using Newtonsoft.Json;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Utils;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace RaceControl.Core.Settings
{
    public class Settings : BindableBase, ISettings
    {
        private static readonly string Filename = FolderUtils.GetLocalApplicationDataFilename("RaceControl.settings.json");

        private readonly ILogger _logger;
        private readonly JsonSerializer _serializer;

        private bool _disableMpvNoBorder;
        private string _recordingLocation = Environment.CurrentDirectory;
        private string _latestRelease;
        private ObservableCollection<string> _selectedSeries;

        public Settings(ILogger logger, JsonSerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public bool DisableMpvNoBorder
        {
            get => _disableMpvNoBorder;
            set => SetProperty(ref _disableMpvNoBorder, value);
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
            if (!File.Exists(Filename))
            {
                return;
            }

            try
            {
                using var file = File.OpenText(Filename);
                _serializer.Populate(file, this);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while loading settings.");
            }

            _logger.Info("Settings loaded.");
        }

        public void Save()
        {
            try
            {
                using var file = File.CreateText(Filename);
                _serializer.Serialize(file, this);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while saving settings.");
            }

            _logger.Info("Settings saved.");
        }
    }
}