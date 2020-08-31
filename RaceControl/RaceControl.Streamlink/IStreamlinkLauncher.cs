using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Task<(Process process, string streamlinkUrl)> StartStreamlinkExternal(string streamUrl, int timeout = 15);

        Process StartStreamlinkRecording(string streamUrl, string title);

        Process StartStreamlinkDownload(string streamUrl, string filename, Action<int> exitAction);

        void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title);

        void StartStreamlinkMpv(string mpvExeLocation, string streamUrl, string title);
    }
}