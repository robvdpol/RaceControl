using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl);

        Process StartStreamlinkRecording(string streamUrl, string title);

        Process StartStreamlinkDownload(string streamUrl, string filename);

        void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title);

        void StartStreamlinkMpv(string mpvExeLocation, string streamUrl, string title);
    }
}