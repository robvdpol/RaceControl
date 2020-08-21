using System.Diagnostics;

namespace RaceControl.Common.ProcessTracker
{
    public interface IChildProcessTracker
    {
        void AddProcess(Process process);
    }
}