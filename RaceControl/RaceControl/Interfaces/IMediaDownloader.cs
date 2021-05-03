using RaceControl.Common.Enums;
using System;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaDownloader : IDisposable
    {
        DownloadStatus Status { get; }

        float Progress { get; }

        Task StartDownloadAsync(string streamUrl, string filename);

        void StopDownload();

        void SetDownloadStatus(DownloadStatus status);
    }
}