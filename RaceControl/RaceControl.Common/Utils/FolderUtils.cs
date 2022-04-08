namespace RaceControl.Common.Utils;

public static class FolderUtils
{
    public static string GetSpecialFolderPath(Environment.SpecialFolder specialFolder)
    {
        return Environment.GetFolderPath(specialFolder);
    }

    public static string GetLocalApplicationDataFilename(string filename)
    {
        return Path.Combine(GetLocalApplicationDataFolder(), filename);
    }

    public static string GetLocalApplicationDataFolder()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceControl");
    }

    public static string GetWebView2UserDataPath()
    {
        return GetLocalApplicationDataFilename("WebView2");
    }

    public static string GetApplicationLogFilePath()
    {
        return GetLocalApplicationDataFilename("RaceControl.log");
    }

    public static string GetFlyleafLogFilePath()
    {
        return GetLocalApplicationDataFilename("Flyleaf.log");
    }
}