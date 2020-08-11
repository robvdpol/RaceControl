using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl);

        Process StartStreamlinkVLC(string vlcExeLocation, string streamUrl);
    }
}