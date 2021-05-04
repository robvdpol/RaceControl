using FlyleafLib.MediaFramework.MediaDemuxer;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaDownloader : BindableBase, IMediaDownloader
    {
        private readonly ILogger _logger;
        private readonly VideoDemuxer _downloader;

        private DownloadStatus _status = DownloadStatus.Pending;
        private float _progress;
        private bool _disposed;

        public FlyleafMediaDownloader(ILogger logger, VideoDemuxer downloader)
        {
            _logger = logger;
            _downloader = downloader;
            _downloader.PropertyChanged += DownloaderOnPropertyChanged;
            _downloader.DownloadCompleted += DownloaderOnDownloadCompleted;
        }

        public DownloadStatus Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public float Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }

        public Task StartDownloadAsync(string streamUrl, string filename)
        {
            return Task.Run(() =>
            {
                var error = _downloader.Open(streamUrl);

                if (error != 0)
                {
                    throw new Exception($"An error occurred while opening the stream URL (error code: {error}).");
                }

                // Only download the highest quality video stream
                if (_downloader.VideoStreams.Any())
                {
                    var videoStream = _downloader.VideoStreams.OrderByDescending(s => s.Height).ThenByDescending(s => s.Width).ThenByDescending(s => s.FPS).First();
                    _downloader.EnableStream(videoStream);
                }

                // Download all audio streams
                foreach (var audioStream in _downloader.AudioStreams)
                {
                    _downloader.EnableStream(audioStream);
                }

                _downloader.Download(filename);
            });
        }

        public void SetDownloadStatus(DownloadStatus status)
        {
            Status = status;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Prevent main application from hanging after closing a download window
                Task.Run(() =>
                {
                    try
                    {
                        _downloader.Stop();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "A non-critical error occurred.");
                    }
                });
            }

            _disposed = true;
        }

        private void DownloaderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_downloader.DownloadPercentage))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Progress = (float)_downloader.DownloadPercentage;
                });
            }
        }

        private void DownloaderOnDownloadCompleted(object sender, bool success)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Status = success ? DownloadStatus.Finished : DownloadStatus.Failed;
            });
        }
    }
}