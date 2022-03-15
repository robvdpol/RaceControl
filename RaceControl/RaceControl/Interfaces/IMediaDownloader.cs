namespace RaceControl.Interfaces;

public interface IMediaDownloader : IDisposable
{
    DownloadStatus Status { get; }

    float Progress { get; }

    Task StartDownloadAsync(string streamUrl, bool isLive, PlayToken playToken, string filename);

    void SetDownloadStatus(DownloadStatus status);
}