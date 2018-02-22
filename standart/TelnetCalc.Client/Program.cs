using System;
using System.Globalization;
using System.Threading;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.ScsServices.Client;

namespace TelnetCalc.Client
{
    internal static class Program
    {
        private static void Main(string[] args)
        {

            //Create a client that can call methods of Calculator Service that is running on local computer and 10083 TCP port
            var port = 10083;
            var host = "127.0.0.1";

            //Since IScsServiceClient is IDisposible, it closes connection at the end of the using block
            using (var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(host, port)))
            {
                //Connect to the server
                client.Connect();
                client.MessageReceived += Client_MessageReceived;

                //console
                Console.Title = $"Client {host}:{port}";
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    client.Disconnect();
                };
                Console.WriteLine($"Server is connected by {port} port");
                Console.WriteLine();
                Console.WriteLine("Commands:");
                Console.WriteLine("'quit' - stop server application");
                Console.WriteLine();

                do
                {

                    var s = Console.ReadLine();

                    try
                    {
                        if (string.IsNullOrWhiteSpace(s))
                            continue; 

                        if (s == "quit")
                        {
                            client.Disconnect();
                            Console.WriteLine($"Server is disconnected by {port} port");

                            break;
                        }

                        if (!int.TryParse(s, NumberStyles.None, null, out var n))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Incorrect format,please try again!");
                            Console.ResetColor();

                            continue;
                        }

                        client.SendMessage(new ScsTextMessage(s));

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    Thread.Sleep(100);

                } while (true);

            } 
        }

        private static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = (ScsTextMessage)e.Message;

            if (!string.IsNullOrWhiteSpace(message.RepliedMessageId))
            {
                Console.WriteLine();
                Console.WriteLine("---------------------------------");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message.Text);
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
}
