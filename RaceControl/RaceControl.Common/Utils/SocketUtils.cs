using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RaceControl.Common.Utils
{
    public static class SocketUtils
    {
        public static int GetFreePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        public static bool IsPortInUse(int port)
        {
            return IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(ep => ep.Port == port);
        }

        public static async Task WaitUntilPortInUseAsync(int port, int timeout, int interval = 200)
        {
            var start = DateTime.UtcNow;

            while (DateTime.UtcNow.Subtract(start).TotalSeconds <= timeout)
            {
                if (IsPortInUse(port))
                {
                    return;
                }

                await Task.Delay(interval);
            }
        }
    }
}