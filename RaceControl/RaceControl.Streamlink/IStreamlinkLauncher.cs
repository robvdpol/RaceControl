using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Task<(Process process, string streamlinkUrl)> StartStreamlinkExternal(string streamUrl, int timeout = 15);

        Process StartStreamlinkRecording(string streamUrl, string title);

        void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title);
    }
}