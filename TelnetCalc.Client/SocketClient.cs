using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Client
{
    public sealed class SocketClient
    {
        private Socket _socket;
        private IPEndPoint _ip;

        public SocketClient(string ip, int port)
        {
            Console.WriteLine($"Connecting to port {port}...");
             
            _ip = new IPEndPoint(IPAddress.Parse(ip), port);

            Console.WriteLine($"Ping... to {ip}:{port}");
        }

        public bool Connect()
        { 
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(_ip);
            }
            catch (SocketException)
            {
                Console.WriteLine("Failed to connect to server.");

                return false;
            }

            Console.WriteLine("Connected.");

            return true;
        }

        public void Send()
        {
            while (true)
            {
                var s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                    continue;

                if (!int.TryParse(s, out var n))
                {
                    Console.WriteLine("Incorrect number format.");
                    continue;
                }

                try
                {
                    _socket.Send(s.GetBytes(), 0, s.Length, SocketFlags.None);

                    Receieve();
                }
                catch (SocketException)
                {
                    Console.WriteLine("Socket connection closed.");

                    continue;
                }

                Thread.Sleep(100);
            }
        }

        //SendReceive#Test4 https://msdn.microsoft.com/en-us/library/w3xtz6a5(v=vs.110).aspx
        private void Receieve()
        {
            var buffer = new byte[1024];
            var size = _socket.Receive(buffer, 0, _socket.Available, SocketFlags.None);

            if (size > 0)
            {
                var result = buffer.GetString();

                Console.WriteLine(result);
            }
        }

        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

    }
}
