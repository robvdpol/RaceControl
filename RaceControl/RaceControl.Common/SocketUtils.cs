using System.Net;
using System.Net.Sockets;

namespace RaceControl.Common
{
    public static class SocketUtils
    {
        public static int GetFreePort()
        {
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));

                return ((IPEndPoint)sock.LocalEndPoint).Port;
            }
        }
    }
}