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

        // Receive buffer.  
        public byte[] Buffer { get; set; }

        // Received data string.  
        public StringBuilder Data { get; set; }

        public StateObject()
        {
            Buffer = new byte[MainSettings.BufferSize];
            Data = new StringBuilder();
        }
    }

}
