using System.Diagnostics;

namespace RaceControl.Common.Utils;

public static class ProcessUtils
{
    public static Process CreateProcess(string filename, string arguments, bool useShellExecute = false, bool createNoWindow = false, bool redirectOutput = false)
    {
        var workingDirectory = Path.GetDirectoryName(filename) ?? Environment.CurrentDirectory;

        return new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = useShellExecute,
                CreateNoWindow = createNoWindow,
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput
            }
        };
    }

    public static void BrowseToUrl(string url)
    {
        using var process = CreateProcess(url, null, true);
        process.Start();
    }
}