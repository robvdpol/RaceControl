using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl);

        void StartStreamlinkVLC(string vlcExeLocation, string streamUrl);
    }
}