using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceControl.Core.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Task<(Process process, string streamlinkUrl)> StartStreamlinkExternalAsync(string streamUrl, int timeout = 15);

        Process StartStreamlinkRecording(string streamUrl, string title);

        void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title);
    }
}