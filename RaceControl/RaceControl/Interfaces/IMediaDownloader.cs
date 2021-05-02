using RaceControl.Common.Enums;
using System;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaDownloader : IDisposable
    {
        Task StartDownloadAsync(string streamUrl, string filename);

        void StopDownload();

        void SetDownloadStatus(DownloadStatus status);
    }
}