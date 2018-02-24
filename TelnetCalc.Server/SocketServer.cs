using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    public sealed class SocketServer
    {
        private readonly int _port;
        private Thread _thread;
        private static readonly IList<Socket> _connections = new List<Socket>();

        public SocketServer(int port)
        {
            _port = port;
        }

        public void Start(Action<Socket> socketCallback)
        {
            _thread = new Thread(new ThreadStart(() =>
            {
                //bind
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
                socket.Listen(MainSettings.MaxConnections);

                Console.WriteLine($"Listening for socket connections on port {_port}...");

                //accept
                while (true)
                {
                    Socket connection;

                    try
                    {
                        connection = socket.Accept();
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Socket accept failed: {ex.Message}");
                        continue;
                    }

                    lock (_connections)
                    {
                        //refuse
                        if (_connections.Count >= MainSettings.MaxConnections)
                        {
                            Shutdown(connection);

                            Console.WriteLine("Socket connection refused.");

                            continue;
                        }
                    }

                    Console.WriteLine("Socket connection accepted.");

                    //Dispatch
                    lock (_connections)
                    {
                        _connections.Add(connection);
                    }

                    try
                    {
                        socketCallback(connection);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Socket connection closed forcibly: {ex.Message}");
                    }
                    finally
                    {
                        Shutdown(connection);

                        Console.WriteLine("Socket connection closed.");
                    }

                    lock (_connections)
                    {
                        _connections.Remove(connection);
                    }

                    Thread.Sleep(100);
                }

            }));
            _thread.Start();
        }

        public void Stop()
        {
            lock (_connections)
            {
                foreach (var connection in _connections)
                {
                    Shutdown(connection);
                }
            }

            _thread.Abort();
        }

        private static void Shutdown(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket could not be shutdown: {ex.Message}");
            }
        }
    }
}
