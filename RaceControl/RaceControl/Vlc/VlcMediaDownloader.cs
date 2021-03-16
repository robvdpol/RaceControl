using LibVLCSharp;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaceControl.Vlc
{
    public class VlcMediaDownloader : BindableBase, IMediaDownloader
    {
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;

        private DownloadStatus _status = DownloadStatus.Pending;
        private float _progress;
        private bool _disposed;

        public VlcMediaDownloader(LibVLC libVLC, MediaPlayer mediaPlayer)
        {
            _libVLC = libVLC;
            _mediaPlayer = mediaPlayer;
            _mediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            _mediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
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

        public async Task StartDownloadAsync(string streamUrl, string filename)
        {
            var option = $":sout=#std{{access=file,mux=ts,dst=\"{filename}\"}}";

            using var media = new Media(_libVLC, streamUrl, FromType.FromLocation, option);
            await media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.FetchNetwork);
            Status = _mediaPlayer.Play(media) ? DownloadStatus.Downloading : DownloadStatus.Failed;
        }

        public void StopDownload()
        {
            _mediaPlayer.Stop();
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
                _mediaPlayer.Dispose();
            }

            _disposed = true;
        }

        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Progress = Math.Min(e.Position * 100, 100);
        }

        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Stop());
            Status = DownloadStatus.Failed;
            Progress = 0;
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Stop());
            Status = DownloadStatus.Finished;
            Progress = 100;
        }
    }
}