using RaceControl.Common;
using RaceControl.Common.Utils;
using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public class StreamlinkLauncher : IStreamlinkLauncher
    {
        private readonly IChildProcessTracker _childProcessTracker;

        public StreamlinkLauncher(IChildProcessTracker childProcessTracker)
        {
            _childProcessTracker = childProcessTracker;
        }

        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl)
        {
            var port = SocketUtils.GetFreePort();
            var process = ProcessUtils.StartProcess(
                @".\streamlink\streamlink.bat",
                $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" best",
                false,
                true);

            _childProcessTracker.AddProcess(process);
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return process;
        }

        public Process StartStreamlinkVLC(string vlcExeLocation, string streamUrl)
        {
            return ProcessUtils.StartProcess(
                @".\streamlink\streamlink.bat",
                $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --hls-audio-select * \"{streamUrl}\" best",
                false,
                true);
        }
    }
}