using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RaceControl.Common.Settings
{
    public class VideoDialogLayout : BindableBase, IVideoDialogLayout
    {
        private const string Filename = "RaceControl.layout.json";

        private readonly ILogger _logger;

        private ObservableCollection<VideoDialogInstance> _instances;

        public VideoDialogLayout(ILogger logger)
        {
            _logger = logger;
        }

        public ObservableCollection<VideoDialogInstance> Instances
        {
            get => _instances ??= new ObservableCollection<VideoDialogInstance>();
            set => SetProperty(ref _instances, value);
        }

        public void Add(IEnumerable<VideoDialogInstance> instances)
        {
            foreach (var instance in instances)
            {
                Instances.Add(instance);
            }
        }

        public void Clear()
        {
            Instances.Clear();
        }

        public void Load()
        {
            _logger.Info("Loading video dialog layout...");

            if (!File.Exists(Filename))
            {
                return;
            }

            try
            {
                using (var file = File.OpenText(Filename))
                {
                    new JsonSerializer().Populate(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while loading video dialog layout.");
            }

            _logger.Info("Done loading video dialog layout.");
        }

        public bool Save()
        {
            _logger.Info("Saving video dialog layout...");

            try
            {
                using (var file = File.CreateText(Filename))
                {
                    new JsonSerializer().Serialize(file, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while saving video dialog layout.");

                return false;
            }

            _logger.Info("Done saving video dialog layout.");

            return true;
        }
    }
}