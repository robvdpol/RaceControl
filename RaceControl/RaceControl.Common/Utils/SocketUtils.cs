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
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            if (socket.LocalEndPoint is IPEndPoint endPoint)
            {
                return endPoint.Port;
            }

            throw new Exception("Could not get a free port.");
        }

        public static async Task WaitUntilPortInUseAsync(int port, int timeoutSeconds, int intervalMilliseconds = 250)
        {
            var start = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);
            var interval = TimeSpan.FromMilliseconds(intervalMilliseconds);

            while (DateTime.UtcNow.Subtract(start) < timeout)
            {
                if (IsPortInUse(port))
                {
                    return;
                }

                await Task.Delay(interval);
            }
        }

        private static bool IsPortInUse(int port)
        {
            return IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(ep => ep.Port == port);
        }
    }
}