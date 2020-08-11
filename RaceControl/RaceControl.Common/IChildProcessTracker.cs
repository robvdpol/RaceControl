using System.Diagnostics;

namespace RaceControl.Common
{
    public interface IChildProcessTracker
    {
        void AddProcess(Process process);
    }
}