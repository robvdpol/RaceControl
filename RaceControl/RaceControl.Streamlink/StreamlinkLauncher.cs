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
        private static readonly string _streamlinkBatchLocation = Path.Combine(Environment.CurrentDirectory, @"streamlink\streamlink.bat");

        private readonly ILogger _logger;
        private readonly IChildProcessTracker _childProcessTracker;

        public StreamlinkLauncher(ILogger logger, IChildProcessTracker childProcessTracker)
        {
            _logger = logger;
            _childProcessTracker = childProcessTracker;
        }

        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title)
        {
            var port = SocketUtils.GetFreePort();
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var recordingParameter = GetRecordingParameter(enableRecording, title);
            var streamlinkArguments = $"--player-external-http --player-external-http-port {port} --hls-audio-select * {recordingParameter} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting external Streamlink-instance for stream-URL '{streamUrl}' on port '{port}' with identifier '{streamIdentifier}'...");

            var process = ProcessUtils.StartProcess(_streamlinkBatchLocation, streamlinkArguments, false, true);
            _childProcessTracker.AddProcess(process);
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return process;
        }

        public Process StartStreamlinkVlc(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title)
        {
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var recordingParameter = GetRecordingParameter(enableRecording, title);
            var streamlinkArguments = $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --hls-audio-select * {recordingParameter} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting VLC Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            return ProcessUtils.StartProcess(_streamlinkBatchLocation, streamlinkArguments, false, true);
        }

        public Process StartStreamlinkMpv(string mpvExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream, bool enableRecording, string title)
        {
            var streamIdentifier = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            var recordingParameter = GetRecordingParameter(enableRecording, title);
            var streamlinkArguments = $"--player \"{mpvExeLocation}\" --hls-audio-select * {recordingParameter} \"{streamUrl}\" {streamIdentifier}";

            _logger.Info($"Starting MPV Streamlink-instance for stream-URL '{streamUrl}' with identifier '{streamIdentifier}'...");

            return ProcessUtils.StartProcess(_streamlinkBatchLocation, streamlinkArguments, false, true);
        }

        private static string GetStreamIdentifier(bool lowQualityMode, bool useAlternativeStream)
        {
            if (!lowQualityMode)
            {
                if (!useAlternativeStream)
                {
                    return "best";
                }
                else
                {
                    return "1080p_alt";
                }
            }
            else
            {
                if (!useAlternativeStream)
                {
                    return "576p";
                }
                else
                {
                    return "576p_alt";
                }
            }
        }

        private string GetRecordingParameter(bool enableRecording, string title)
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
                _logger.Error(ex, "An exception occurred while getting the recording filename.");

                return null;
            }

            return $"--record \"{recordingFilename}\"";
        }

        private string GetRecordingFilename(string title)
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

            var recordingFilename = Path.Combine(recordingsDirectory, $"{DateTime.Now:yyyyMMddHHmmss} {title}.mkv");

            if (File.Exists(recordingFilename))
            {
                try
                {
                    File.Delete(recordingFilename);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not delete existing recording '{recordingFilename}'.", ex);
                }
            }

            return recordingFilename;
        }
    }
}