using GoogleCast;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Zeroconf;

namespace RaceControl.GoogleCast
{
    public class CustomDeviceLocator : DeviceLocator, ICustomDeviceLocator
    {
        private const string Protocol = "_googlecast._tcp.local.";

        private static Receiver CreateReceiver(IZeroconfHost host)
        {
            var service = host.Services[Protocol];
            var properties = service.Properties.First();

            return new Receiver
            {
                Id = properties["id"],
                FriendlyName = properties["fn"],
                IPEndPoint = new IPEndPoint(IPAddress.Parse(host.IPAddress), service.Port)
            };
        }

        public async Task<IEnumerable<IReceiver>> FindReceiversAsync(NetworkInterface networkInterface)
        {
            return (await ZeroconfResolver.ResolveAsync(Protocol, netInterfacesToSendRequestOn: new[] { networkInterface })).Select(CreateReceiver);
        }
    }
}