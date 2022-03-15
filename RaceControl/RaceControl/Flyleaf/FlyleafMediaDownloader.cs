namespace RaceControl.Flyleaf;

public class FlyleafMediaDownloader : BindableBase, IMediaDownloader
{
    private readonly ISettings _settings;
    private readonly Downloader _downloader;

    private DownloadStatus _status = DownloadStatus.Pending;
    private float _progress;
    private bool _disposed;

    public FlyleafMediaDownloader(ISettings settings, Downloader downloader)
    {
        _settings = settings;
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

    public Task StartDownloadAsync(string streamUrl, bool isLive, PlayToken playToken, string filename)
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

            var videoStreams = _downloader.DecCtx.VideoDemuxer.VideoStreams;

            if (videoStreams.Any())
            {
                var videoStream = videoStreams.GetVideoStreamForQuality(_settings.DefaultVideoQuality) ?? videoStreams.OrderByDescending(s => s.Height).ThenByDescending(s => s.Width).ThenByDescending(s => s.FPS).First();
                _downloader.DecCtx.VideoDemuxer.EnableStream(videoStream);
            }

            // Download all audio streams
            foreach (var audioStream in _downloader.DecCtx.VideoDemuxer.AudioStreams)
            {
                _downloader.DecCtx.VideoDemuxer.EnableStream(audioStream);
            }

            // Always start from the beginning (needed for live sessions)
            if (isLive)
            {
                _downloader.DecCtx.Seek(0);
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