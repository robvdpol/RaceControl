using System;
using System.IO;

namespace RaceControl.Common.Utils
{
    public static class FolderUtils
    {
        public static string GetLocalApplicationDataFolder()
        {
            var localApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(localApplicationDataPath, "RaceControl");
        }

        public static string GetLocalApplicationDataFileName(string filename)
        {
            return Path.Combine(GetLocalApplicationDataFolder(), filename);
        }
    }
}