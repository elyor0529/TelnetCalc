using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    internal class Program
    {
        private static int Main(String[] args)
        {
            var server = new SocketServer(11000);
            server.Start();

            return 0;
        }
    }
}
