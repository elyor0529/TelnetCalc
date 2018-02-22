using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Service;

namespace TelnetCalc.Server
{
    internal static class Program
    {
        private readonly static IDictionary<long, int> _numbers = new Dictionary<long, int>();

        private static void Main(string[] args)
        {
            var port = 10083;

            //Create a service application that runs on 10083 TCP port
            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(port));

            //Start service application
            server.Start();

            //events
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
             
            //console
            Console.Title = $"Server 127.0.0.1:{port}";
            Console.WriteLine($"Server is connected by {port} port");
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                server.Stop();
            };
            Console.WriteLine("Press enter to stop server application");

            while (true)
            {
                var s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                    break;

                Thread.Sleep(100);
            }

            //Stop service application
            server.Stop();

        }

        private static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var client = (IScsServerClient)sender;
            var id = client.ClientId;

            if (_numbers.ContainsKey(id))
                _numbers.Remove(id);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Client#{e.Client.ClientId} is disconnected");
            Console.ResetColor();

        }

        private static void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var id = e.Client.ClientId;

            if (!_numbers.ContainsKey(id))
                _numbers.Add(id, 0);

            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.MessageSent += Client_MessageSent;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Client#{e.Client.ClientId} is connected. Address: " + e.Client.RemoteEndPoint);
            Console.ResetColor();

        }

        private static void Client_MessageSent(object sender, MessageEventArgs e)
        {
            var message = (ScsTextMessage)e.Message;
            var client = (IScsServerClient)sender;
            var id = client.ClientId;

            Console.WriteLine($"Client#{id}: {message.Text}");
        }

        private static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = (ScsTextMessage)e.Message;
            var client = (IScsServerClient)sender;
            var id = client.ClientId;

            if (int.TryParse(message.Text, NumberStyles.None, null, out var n))
            {
                _numbers[id] += n;
            }

            Console.WriteLine();
            Console.WriteLine("---------------");
            Console.WriteLine("Total sum: {0}", _numbers.Sum(a => a.Value));
            Console.WriteLine();

            client.SendMessage(new ScsTextMessage("Total sum: " + _numbers[id])
            {
                RepliedMessageId = Guid.NewGuid().ToString()
            });
        }
    }
}