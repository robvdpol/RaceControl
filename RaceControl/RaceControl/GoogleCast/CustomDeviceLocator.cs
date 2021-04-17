using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using GoogleCast;
using Zeroconf;

namespace RaceControl.GoogleCast
{
    public class CustomDeviceLocator : IDeviceLocator
    {
        private const string PROTOCOL = "_googlecast._tcp.local.";

        private Receiver CreateReceiver(IZeroconfHost host)
        {
            var service = host.Services[PROTOCOL];
            var properties = service.Properties.First();
            return new Receiver()
            {
                Id = properties["id"],
                FriendlyName = properties["fn"],
                IPEndPoint = new IPEndPoint(IPAddress.Parse(host.IPAddress), service.Port)
            };
        }

        public Task<IEnumerable<IReceiver>> FindReceiversAsync()
        {
            throw new NotImplementedException();
        }

        public IObservable<IReceiver> FindReceiversContinuous()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IReceiver>> FindReceiversAsync(NetworkInterface networkInterface)
        {
            return (await ZeroconfResolver.ResolveAsync(PROTOCOL, netInterfacesToSendRequestOn: new NetworkInterface[] {networkInterface} )).Select(CreateReceiver);
        }
    }
}
