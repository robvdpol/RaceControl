using System;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaDownloader : IDisposable
    {
        Task StartDownloadAsync(string streamUrl, string filename);

        void SetFailedStatus();
    }
}