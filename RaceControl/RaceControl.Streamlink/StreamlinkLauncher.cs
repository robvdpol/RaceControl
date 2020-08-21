using NLog;
using RaceControl.Common.ProcessTracker;
using RaceControl.Common.Settings;
using RaceControl.Common.Utils;
using System;
using System.Diagnostics;
using System.IO;

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

        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl)
        {
            var port = SocketUtils.GetFreePort();
            var streamIdentifier = GetStreamIdentifier();
            var streamlinkArguments = $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";
            streamlinkUrl = $"http://127.0.0.1:{port}";

            _logger.Info($"Starting external Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}' on port '{port}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, false, true);
            process.Start();
            _childProcessTracker.AddProcess(process);

            return process;
        }

        public Process StartStreamlinkRecording(string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var recordingFilename = GetRecordingFilename(title);
            var streamlinkArguments = $"--output \"{recordingFilename}\" --force --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting recording Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}' to file '{recordingFilename}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, false, true);
            process.Start();
            _childProcessTracker.AddProcess(process);

            return process;
        }

        public Process StartStreamlinkDownload(string streamUrl, string filename)
        {
            var streamlinkArguments = $"--output \"{filename}\" --force --hls-audio-select * \"{streamUrl}\" best";

            _logger.Info($"Starting download Streamlink-instance for stream-URL '{streamUrl}' to file '{filename}'...");

            var process = ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, false, true, true);
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, args) => _logger.Info(args.Data);
            process.ErrorDataReceived += (sender, args) => _logger.Error(args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _childProcessTracker.AddProcess(process);

            return process;
        }

        public void StartStreamlinkVlc(string vlcExeLocation, string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var recordingArguments = GetRecordingArguments(title);
            var streamlinkArguments = $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --title \"{title}\" --hls-audio-select * {recordingArguments} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting VLC Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, false, true).Start();
        }

        public void StartStreamlinkMpv(string mpvExeLocation, string streamUrl, string title)
        {
            var streamIdentifier = GetStreamIdentifier();
            var recordingArguments = GetRecordingArguments(title);
            var streamlinkArguments = $"--player \"{mpvExeLocation}\" --title \"{title}\" --hls-audio-select * {recordingArguments} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting MPV Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            ProcessUtils.CreateProcess(StreamlinkBatchLocation, streamlinkArguments, false, true).Start();
        }

        private string GetStreamIdentifier()
        {
            if (_settings.LowQualityMode)
            {
                return !_settings.UseAlternativeStream ? "576p" : "576p_alt";
            }

            return !_settings.UseAlternativeStream ? "best" : "1080p_alt";
        }

        private string GetRecordingArguments(string title)
        {
            if (!_settings.EnableRecording)
            {
                return string.Empty;
            }

            var recordingFilename = GetRecordingFilename(title);

            return $"--record \"{recordingFilename}\" --force";
        }

        private string GetRecordingFilename(string title)
        {
            var filename = $"{DateTime.Now:yyyyMMddHHmmss} {title}.mkv".RemoveInvalidFileNameChars(string.Empty);

            return Path.Combine(_settings.RecordingLocation, filename);
        }
    }
}