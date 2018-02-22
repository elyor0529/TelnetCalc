using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TelnetCalc.Core.Common
{
    /// <summary>
    /// A implementation of <see cref="IConnectionProxy"/>
    /// which provides access to an underlying <see cref="Socket"/>.
    /// </summary>
    public class ConnectionProxy : IConnectionProxy
    {
        private readonly Socket _socket;

        public ConnectionProxy(Socket socket)
        {
            _socket = socket;
        }

        public int Receive(byte[] buffer, int offset, int size)
        {
            return _socket.Receive(buffer, offset, size, SocketFlags.None);
        }
    }
}
