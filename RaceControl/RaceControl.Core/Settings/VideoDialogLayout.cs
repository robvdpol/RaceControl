using Newtonsoft.Json;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Utils;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace RaceControl.Core.Settings
{
    public class VideoDialogLayout : BindableBase, IVideoDialogLayout
    {
        private static readonly string Filename = FolderUtils.GetLocalApplicationDataFilename("RaceControl.layout.json");

        private readonly ILogger _logger;
        private readonly JsonSerializer _serializer;

        private ObservableCollection<VideoDialogSettings> _instances;

        public VideoDialogLayout(ILogger logger, JsonSerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public ObservableCollection<VideoDialogSettings> Instances
        {
            get => _instances ??= new ObservableCollection<VideoDialogSettings>();
            set => SetProperty(ref _instances, value);
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
                _logger.Error(ex, "An error occurred while loading video dialog layout.");

                return;
            }

            _logger.Info("Video dialog layout loaded.");
        }

        public bool Save()
        {
            try
            {
                using var file = File.CreateText(Filename);
                _serializer.Serialize(file, this);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while saving video dialog layout.");

                return false;
            }

            _logger.Info("Video dialog layout saved.");

            return true;
        }
    }
}