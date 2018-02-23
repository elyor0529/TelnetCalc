using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Client
{
    /// <summary>
    ///Asynchronous Client Socket Example 
    /// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
    /// </summary>
    public class SocketClient
    {
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent _connected = new ManualResetEvent(false);
        private ManualResetEvent _sended = new ManualResetEvent(false);
        private ManualResetEvent _received = new ManualResetEvent(false);

        // The response from the remote device.  
        private string _response;
        private string _host;
        private int _port;

        /// <summary>
        /// The port number for the remote device.  
        /// </summary>
        /// <param name="port"></param>
        public SocketClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the  remote device is "host".  
                var ipHostInfo = Dns.GetHostEntry(_host);
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEP = new IPEndPoint(ipAddress, _port);

                // Create a TCP/IP socket.  
                var client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                _connected.WaitOne();

                // Send test data to the remote device.  
                Send(client, "This is a test<EOF>");
                _sended.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                _received.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", _response);

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                _connected.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                var state = new StateObject
                {
                    Socket = client
                };

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, MainSettings.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                var state = (StateObject)ar.AsyncState;
                var client = state.Socket;

                // Read data from the remote device.  
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.Data.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.Buffer, 0, MainSettings.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.Data.Length > 1)
                    {
                        _response = state.Data.ToString();
                    }

                    // Signal that all bytes have been received.  
                    _received.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = client.EndSend(ar);

                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                _sended.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
