using System;
using System.IO;

namespace RaceControl.Common.Utils
{
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
    }
}