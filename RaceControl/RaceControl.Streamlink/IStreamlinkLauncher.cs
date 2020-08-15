using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream);

        Process StartStreamlinkVLC(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream);
    }
}