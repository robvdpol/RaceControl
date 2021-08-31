using RaceControl.Common.Enums;
using RaceControl.Services.Interfaces.F1TV.Api;
using System;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaDownloader : IDisposable
    {
        DownloadStatus Status { get; }

        float Progress { get; }

        Task StartDownloadAsync(string streamUrl, PlayToken playToken, string filename);

        void SetDownloadStatus(DownloadStatus status);
    }
}