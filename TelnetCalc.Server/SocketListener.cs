using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TelnetCalc.Server
{
    public class SocketListener
    {
        private readonly int _port;
        private  const int MaxConnections=100;
        private Socket _socket;
        private readonly IList<Socket> _connections = new List<Socket>();
        private readonly object _connectionsLock = new object();

        public SocketListener(int port )
        {
            _port = port; 
        }

        public void Start(Action<Socket> newSocketConnectionCallback)
        {
            // Start thread for server socket that will listen for
            // new connections.
            var thread = new Thread(new ThreadStart(() =>
            {
                BindAndListen(newSocketConnectionCallback);
            }));
            thread.Start();
        }

        public void Stop()
        {
            lock (_connectionsLock)
            {
                // Close socket connections (thereby release threads)
                // for each open connection.
                foreach (var connection in _connections)
                {
                    ShutdownSocket(connection);
                }

            }

            if (_socket != null)
            {
                // Close server socket and stop listening on port.
                // Will release the thread on which that's running.
                _socket.Dispose();
                _socket = null;
            }
        }

        private void BindAndListen(Action<Socket> newSocketConnectionCallback)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
            _socket.Listen(MaxConnections);

            Console.WriteLine($"Listening for socket connections on port {_port}...");

            BlockAndAcceptConnections(newSocketConnectionCallback);
        }

        private void BlockAndAcceptConnections(Action<Socket> newSocketConnectionCallback)
        {
            while (_socket != null)
            {
                Socket connection;

                try
                {
                    // Blocking method
                    connection = _socket.Accept();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket accept failed: {ex.Message}");
                    continue;
                }

                if (ShouldRefuseConnection())
                {
                    // We already have the max number of connections.
                    ShutdownSocket(connection);
                    Console.WriteLine("Socket connection refused.");

                    continue;
                }

                Console.WriteLine("Socket connection accepted.");

                DispatchThreadForNewConnection(connection, newSocketConnectionCallback);
            }
        }

        private bool ShouldRefuseConnection()
        {
            lock (_connectionsLock)
            {
                return _connections.Count >= MaxConnections;
            }
        }

        private void DispatchThreadForNewConnection(Socket connection, Action<Socket> newSocketConnectionCallback)
        {
            // Create thread to manage new socket connection.
            // Will stay alive as long as callback is executing.
            var thread = new Thread(new ThreadStart(() =>
            {
                ExecuteCallback(connection, newSocketConnectionCallback);

                lock (_connectionsLock)
                {
                    _connections.Remove(connection);
                }
            }));
            thread.Start();

            lock (_connectionsLock)
            {
                _connections.Add(connection);
            }
        }

        private static void ExecuteCallback(Socket connection, Action<Socket> newSocketConnectionCallback)
        {
            try
            {
                newSocketConnectionCallback(connection);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket connection closed forcibly: {ex.Message}");
            }
            finally
            {
                ShutdownSocket(connection);
                Console.WriteLine("Socket connection closed.");
            }
        }

        private static void ShutdownSocket(Socket socket)
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
