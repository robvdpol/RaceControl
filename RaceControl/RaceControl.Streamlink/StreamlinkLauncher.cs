using NLog;
using RaceControl.Common.ProcessTracker;
using RaceControl.Common.Settings;
using RaceControl.Common.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RaceControl.Streamlink
{
    public class StreamlinkLauncher : IStreamlinkLauncher
    {
        private static readonly string StreamlinkBatchLocation = Path.Combine(Environment.CurrentDirectory, @"streamlink\streamlink.bat");

        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private readonly IChildProcessTracker _childProcessTracker;

        public StreamlinkLauncher(ILogger logger, ISettings videoSettings, IChildProcessTracker childProcessTracker)
        {
            _logger = logger;
            _settings = videoSettings;
            _childProcessTracker = childProcessTracker;
        }

        public async Task<(Process process, string streamlinkUrl)> StartStreamlinkExternal(string streamUrl, int timeout = 15)
        {
            var port = SocketUtils.GetFreePort();
            var streamIdentifier = GetStreamIdentifier();
            var streamlinkArguments = $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting external Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}' on port '{port}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, true, true);
            process.OutputDataReceived += (sender, args) => _logger.Info($"[Streamlink] {args.Data}");
            process.Start();
            process.BeginOutputReadLine();
            _childProcessTracker.AddProcess(process);
            await SocketUtils.WaitUntilPortInUseAsync(port, timeout);

            return (process, $"http://127.0.0.1:{port}");
        }

        public Process StartStreamlinkRecording(string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var filename = GetRecordingFilename(title);
            var streamlinkArguments = $"--output \"{filename}\" --force --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting recording Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}' to file '{filename}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, true, true);
            process.OutputDataReceived += (sender, args) => _logger.Info($"[Streamlink] {args.Data}");
            process.Start();
            process.BeginOutputReadLine();
            _childProcessTracker.AddProcess(process);

            return process;
        }

        public void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var streamlinkArguments = $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --title \"{title}\" --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting VLC Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, true);
            process.Start();
            process.Dispose();
        }

        public void StartStreamlinkMpv(string mpvExeLocation, string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var streamlinkArguments = $"--player \"{mpvExeLocation}\" --title \"{title}\" --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting MPV Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, true);
            process.Start();
            process.Dispose();
        }

        private string GetStreamIdentifier()
        {
            if (_settings.LowQualityMode)
            {
                return !_settings.UseAlternativeStream ? "576p" : "576p_alt";
            }

            return !_settings.UseAlternativeStream ? "best" : "1080p_alt";
        }

        private string GetRecordingFilename(string title)
        {
            var filename = $"{DateTime.Now:yyyy-MM-dd HH.mm.ss.fff} {title}.ts".RemoveInvalidFileNameChars();

            return Path.Combine(_settings.RecordingLocation, filename);
        }
    }
}