using System.Diagnostics;

namespace RaceControl.Common.Utils
{
    public static class ProcessUtils
    {
        public static Process CreateProcess(string filename, string arguments, bool useShellExecute = false, bool createNoWindow = false, bool redirectOutput = false)
        {
            return new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = useShellExecute,
                    CreateNoWindow = createNoWindow,
                    RedirectStandardOutput = redirectOutput,
                    RedirectStandardError = redirectOutput
                }
            };
        }

        public static void BrowseToUrl(string url)
        {
            Process.Start("explorer.exe", url)?.Dispose();
        }
    }
}