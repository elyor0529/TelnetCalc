using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TelnetCalc.Common
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket Socket { get; set; }

        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] Buffer { get; set; }

        // Received data string.  
        public StringBuilder Data { get; set; }

        public StateObject()
        {
            Buffer = new byte[BufferSize];
            Data = new StringBuilder();
        }
    }

}
