using GoogleCast;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RaceControl.GoogleCast
{
    public interface ICustomDeviceLocator : IDeviceLocator
    {
        Task<IEnumerable<IReceiver>> FindReceiversAsync(NetworkInterface networkInterface);
    }
}