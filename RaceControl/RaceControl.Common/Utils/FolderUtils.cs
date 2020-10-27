using System;
using System.IO;

namespace RaceControl.Common.Utils
{
    public static class FolderUtils
    {
        public static string GetLocalApplicationDataFilename(string filename)
        {
            return Path.Combine(GetLocalApplicationDataFolder(), filename);
        }

        private static string GetLocalApplicationDataFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceControl");
        }
    }
}