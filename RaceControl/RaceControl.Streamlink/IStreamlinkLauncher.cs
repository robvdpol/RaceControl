using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream);

        Process StartStreamlinkVlc(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream);

        Process StartStreamlinkMpv(string mpvExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream);
    }
}