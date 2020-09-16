using System;
using System.IO;

namespace RaceControl.Common.Utils
{
    public static class FolderUtils
    {
        public static string GetLocalApplicationDataFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceControl");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetLocalApplicationDataFilename(string filename)
        {
            return Path.Combine(GetLocalApplicationDataFolder(), filename);
        }
    }
}