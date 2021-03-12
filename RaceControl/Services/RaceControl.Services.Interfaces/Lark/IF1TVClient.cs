using RaceControl.Services.Interfaces.F1TV.Api;
using System.Threading.Tasks;

namespace RaceControl.Services.Interfaces.Lark
{
    public interface IF1TVClient : ILarkClient
    {
        Task<BackupStream> GetBackupStream();
    }
}