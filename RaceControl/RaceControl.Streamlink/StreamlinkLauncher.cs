using NLog;
using RaceControl.Common;
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
        private readonly IChildProcessTracker _childProcessTracker;

        public StreamlinkLauncher(ILogger logger, IChildProcessTracker childProcessTracker)
        {
            _logger = logger;
            _childProcessTracker = childProcessTracker;
        }

        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream)
        {
            var port = SocketUtils.GetFreePort();
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var streamlinkArguments = $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting external Streamlink-instance for stream-URL '{streamUrl}' on port '{port}' with identifier '{streamIdentifier}'...");

            var process = ProcessUtils.StartProcess(StreamlinkBatchLocation, streamlinkArguments, false, true);
            _childProcessTracker.AddProcess(process);
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return process;
        }

        public Process StartStreamlinkVlc(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title)
        {
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var recordingArguments = GetRecordingArguments(enableRecording, title);
            var streamlinkArguments = $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --title \"{title}\" --hls-audio-select * {recordingArguments} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting VLC Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            return ProcessUtils.StartProcess(StreamlinkBatchLocation, streamlinkArguments, false, true);
        }

        public Process StartStreamlinkMpv(string mpvExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title)
        {
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var recordingArguments = GetRecordingArguments(enableRecording, title);
            var streamlinkArguments = $"--player \"{mpvExeLocation}\" --title \"{title}\" --hls-audio-select * {recordingArguments} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting MPV Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            return ProcessUtils.StartProcess(StreamlinkBatchLocation, streamlinkArguments, false, true);
        }

        private static string GetStreamIdentifier(bool lowQualityMode, bool useAlternativeStream)
        {
            if (lowQualityMode)
            {
                return !useAlternativeStream ? "576p" : "576p_alt";
            }

            return !useAlternativeStream ? "best" : "1080p_alt";
        }

        private string GetRecordingArguments(bool enableRecording, string title)
        {
            if (!enableRecording)
            {
                return null;
            }

            string recordingFilename;

            try
            {
                recordingFilename = GetRecordingFilename(title);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return null;
            }

            return $"--record \"{recordingFilename}\" --force";
        }

        private static string GetRecordingFilename(string title)
        {
            var recordingsDirectory = Path.Combine(Environment.CurrentDirectory, "Recordings");

            if (!Directory.Exists(recordingsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(recordingsDirectory);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not create recordings-directory '{recordingsDirectory}'.", ex);
                }
            }

            return Path.Combine(recordingsDirectory, $"{DateTime.Now:yyyyMMddHHmmss} {title}.mkv");
        }
    }
}