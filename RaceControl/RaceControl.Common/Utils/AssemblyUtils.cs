using System.Reflection;

namespace RaceControl.Common.Utils;

public static class AssemblyUtils
{
    public static Version GetApplicationVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0);
    }
}