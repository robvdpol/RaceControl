using FlyleafLib.MediaFramework.MediaDemuxer;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaDownloader : BindableBase, IMediaDownloader
    {
        private readonly VideoDemuxer _downloader;

        private DownloadStatus _status = DownloadStatus.Pending;
        private float _progress;
        private bool _disposed;

        public FlyleafMediaDownloader(VideoDemuxer downloader)
        {
            _downloader = downloader;
            _downloader.PropertyChanged += DownloaderOnPropertyChanged;
            _downloader.DownloadCompleted += DownloaderOnDownloadCompleted;
        }

        public DownloadStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public float Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public Task StartDownloadAsync(string streamUrl, string filename)
        {
            return Task.Run(() =>
            {
                var result = _downloader.Open(streamUrl);

                if (result != 0)
                {
                    Status = DownloadStatus.Failed;
                    return;
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

        public void StopDownload()
        {
            _downloader.Stop();
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
                _downloader.Stop();
            }

            _disposed = true;
        }

        private void DownloaderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_downloader.DownloadPercentage))
            {
                Progress = (float)_downloader.DownloadPercentage;
            }
        }

        private void DownloaderOnDownloadCompleted(object sender, bool success)
        {
            Status = success ? DownloadStatus.Finished : DownloadStatus.Failed;
        }
    }
}