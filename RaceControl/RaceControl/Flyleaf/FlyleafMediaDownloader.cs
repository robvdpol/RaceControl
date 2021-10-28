using FlyleafLib.MediaFramework.MediaContext;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Api;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaDownloader : BindableBase, IMediaDownloader
    {
        private readonly Downloader _downloader;

        private DownloadStatus _status = DownloadStatus.Pending;
        private float _progress;
        private bool _disposed;

        public FlyleafMediaDownloader(Downloader downloader)
        {
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

        public Task StartDownloadAsync(string streamUrl, PlayToken playToken, string filename)
        {
            return Task.Run(() =>
            {
                if (playToken != null)
                {
                    _downloader.DecCtx.Config.Demuxer.FormatOpt.Add("headers", playToken.GetCookieString());
                }

                var error = _downloader.Open(streamUrl, true, false, false);

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"An error occurred while opening the stream URL (error: '{error}').");
                }

                // Only download the highest quality video stream
                if (_downloader.DecCtx.VideoDemuxer.VideoStreams.Any())
                {
                    var videoStream = _downloader.DecCtx.VideoDemuxer.VideoStreams.OrderByDescending(s => s.Height).ThenByDescending(s => s.Width).ThenByDescending(s => s.Fps).First();
                    _downloader.DecCtx.VideoDemuxer.EnableStream(videoStream);
                }

                // Download all audio streams
                foreach (var audioStream in _downloader.DecCtx.VideoDemuxer.AudioStreams)
                {
                    _downloader.DecCtx.VideoDemuxer.EnableStream(audioStream);
                }

                // Selected filename will already have MP4-extension
                _downloader.Download(ref filename, false);
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
                _downloader.Dispose();
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