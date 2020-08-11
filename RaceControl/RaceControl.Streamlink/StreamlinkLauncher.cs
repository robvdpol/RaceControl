using RaceControl.Common;
using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public class StreamlinkLauncher : IStreamlinkLauncher
    {
        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl)
        {
            var port = SocketUtils.GetFreePort();
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = @".\streamlink\streamlink.bat",
                Arguments = $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" best",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            ChildProcessTracker.AddProcess(process);
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return process;
        }

        public void StartStreamlinkVLC(string vlcExeLocation, string streamUrl)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = @".\streamlink\streamlink.bat",
                Arguments = $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --hls-audio-select * \"{streamUrl}\" best",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}