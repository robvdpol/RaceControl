using System.Diagnostics;

namespace RaceControl.Common.Utils
{
    public static class ProcessUtils
    {
        public static Process StartProcess(string filename, string arguments, bool useShellExecute = true, bool createNoWindow = false)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                CreateNoWindow = createNoWindow
            });
        }

        public static Process BrowseToUrl(string url)
        {
            return Process.Start("explorer.exe", url);
        }
    }
}