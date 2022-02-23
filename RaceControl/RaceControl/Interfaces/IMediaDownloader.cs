namespace RaceControl.Interfaces;

public interface IMediaDownloader : IDisposable
{
    DownloadStatus Status { get; }

    float Progress { get; }

    Task StartDownloadAsync(string streamUrl, PlayToken playToken, string filename);

    void SetDownloadStatus(DownloadStatus status);
}
