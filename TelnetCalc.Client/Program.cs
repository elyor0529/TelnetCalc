using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TelnetCalc.Client
{
    class Program
    {
        public static int Main(String[] args)
        {
            var client = new SocketClient("127.0.0.1", 11000);

            client.Connect();

            return 0;
        }
    }
}
