using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Client
{
    internal class Program
    {
        private static Socket _socket;
        private static bool _exit;

        public static int Main(string[] args)
        {
            var port = 11000;
            var ip = "127.0.0.1";
            var cmd = new CommandLineApplication()
            {
                FullName = "A console application that will send data to the number server.",
                Name = "dotnet run --"
            };
            var ipOption = cmd.Option("-ip|--ip <ip>", $"The ip on which the server is running. Default: {ip}", CommandOptionType.SingleValue);
            var portOption = cmd.Option("-port|--port <port>", $"The port on which the server is running. Default: {port}", CommandOptionType.SingleValue);

            cmd.HelpOption("-h|--help"); 
            cmd.OnExecute(() =>
            {
                if (portOption.HasValue())
                {
                    int.TryParse(portOption.Value(), out port);
                }
                if (ipOption.HasValue())
                {
                    if (IPAddress.TryParse(ipOption.Value(), out var ipAddress))
                    {
                        ip = ipOption.Value();
                    }
                }

                return Run(ip, port);
            });

            return cmd.Execute(args);
        }

        private static int Run(string ip, int port)
        {
            if (!ConnectToServer(ip, port))
            {
                return 1;
            }

            Console.CancelKeyPress += delegate
            {
                _exit = true;
                DisconnectFromServer();
            };

            SendData();
            DisconnectFromServer();

            return 0;
        }

        private static bool ConnectToServer(string ip, int port)
        {
            Console.WriteLine($"Connecting to port {port}...");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var ipHostInfo = Dns.GetHostEntry(ip);
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEP = new IPEndPoint(ipAddress, port);

                Console.WriteLine($"Ping to {ip}:{port}");

                _socket.Connect(remoteEP);
            }
            catch (SocketException)
            {
                Console.WriteLine("Failed to connect to server.");

                return false;
            }

            Console.WriteLine("Connected.");

            return true;
        }

        private static void SendData()
        {
            while (!_exit)
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
                    _socket.Send(n.ToBytes());
                }
                catch (SocketException)
                {
                    Console.WriteLine("Socket connection closed.");

                    continue;
                }

                Thread.Sleep(100);
            }
        }

        private static void DisconnectFromServer()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

    }
}
