using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public interface IStreamlinkLauncher
    {
        Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream);

        Process StartStreamlinkRecording(string streamUrl, bool lowQualityMode, bool useAlternativeStream, string title);

        void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title);

        void StartStreamlinkMpv(string mpvExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title);
    }
}