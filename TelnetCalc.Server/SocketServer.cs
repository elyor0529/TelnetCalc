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
        private Socket _socket;
        private static object _locker = new object();
        private readonly IList<Socket> _connections = new List<Socket>();
        private readonly IDictionary<int, long> _values = new Dictionary<int, long>();
        private int _id;

        public SocketServer(int port)
        {
            _port = port;
        }

        public void Start()
        {

            //bind
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = MainSettings.TimeOut
            };
            _socket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
            _socket.Listen(MainSettings.MaxConnections);

            Console.WriteLine($"Listening for socket connections on port {_port}...");

            //accept
            while (_socket != null)
            {
                Socket connection;

                try
                {
                    connection = _socket.Accept();
                }
                catch (SocketException exp)
                {
                    Console.WriteLine(exp.Message);
                    continue;
                }

                lock (_locker)
                {
                    //refuse
                    if (_connections.Count >= MainSettings.MaxConnections)
                    {
                        Shutdown(connection);

                        Console.WriteLine("Socket connection refused.");

                        continue;
                    }
                }

                //Dispatch
                lock (_locker)
                {
                    _connections.Add(connection);
                }

                try
                {
                    SocketCallback(connection);
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
                finally
                {
                    Shutdown(connection);

                    Console.WriteLine("Socket connection closed.");
                }

                lock (_locker)
                {
                    _connections.Remove(connection);
                }

                Thread.Sleep(100);
            }
        }

        private void SocketCallback(Socket socket)
        {

            _id++;

            lock (_locker)
            {
                if (!_values.ContainsKey(_id))
                {
                    _values.Add(_id, 0);
                }
            }

            var buffer = new byte[MainSettings.ChunkSize];
            var bytesRead = 0;

            while (true)
            {
                bytesRead = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                var data = buffer.GetString();

                if (int.TryParse(data, out var value))
                {

                    lock (_locker)
                    {
                        _values[_id] += value;

                        Console.WriteLine("Client#{0}: {1}",_id, _values[_id]);

                        var s = _values[_id] + Environment.NewLine;

                        socket.Send(s.GetBytes(), 0, s.Length, SocketFlags.None);
                    }
                }

                Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                foreach (var connection in _connections)
                {
                    Shutdown(connection);
                }
                _connections.Clear();

                _values.Clear();
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
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
