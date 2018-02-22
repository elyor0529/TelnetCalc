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

            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("'quit' - stop server application");
            Console.WriteLine("'list' - list of clients");
            Console.WriteLine();

            while (true)
            {
                var s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s))
                    continue;

                switch (s)
                {
                    case "quit":
                        {

                            //Stop service application
                            server.Stop();
                        }
                        break;

                    case "list":
                        {
                            var clients = server.Clients.GetAllItems();

                            Console.WriteLine();
                            for (var i = 0; i < clients.Count; i++)
                            {
                                var id = clients[i].ClientId;

                                Console.WriteLine("Client#{0}: {1}", id, _numbers[id]);
                            }
                            Console.WriteLine("---------------");
                            Console.WriteLine("Total: {0}", _numbers.Sum(a => a.Value));
                            Console.WriteLine();

                        }
                        break;

                    default:
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Incorrect command,please try again!");
                            Console.ResetColor();
                        }
                        break;
                } 

                Thread.Sleep(100);
            } 

        }

        private static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var id = e.Client.ClientId;

            if (_numbers.ContainsKey(id))
                _numbers.Remove(id);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Client#{id} is disconnected");
            Console.ResetColor();

        }

        private static void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var id = e.Client.ClientId;

            if (!_numbers.ContainsKey(id))
                _numbers.Add(id, 0);

            e.Client.MessageReceived += Client_MessageReceived;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Client#{e.Client.ClientId} is connected. Address: " + e.Client.RemoteEndPoint);
            Console.ResetColor();

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

            client.SendMessage(new ScsTextMessage("Sum: " + _numbers[id])
            {
                RepliedMessageId = Guid.NewGuid().ToString()
            });

        }
    }
}