using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    internal class Program
    {
        private static SocketListener _listener;
        private static ManualResetEventSlim _exitSignal = new ManualResetEventSlim();

        static void Main(string[] args)
        {
            var port = 11000;

            var cmd = new CommandLineApplication()
            {
                FullName = "Server Socket application.",
                Name = "dotnet run --"
            };
            var portOption = cmd.Option("-p|--port <port>", $"Port. Default: {port}", CommandOptionType.SingleValue);
            cmd.HelpOption("-?|-h|--help");
            cmd.OnExecute(() =>
            {
                if (portOption.HasValue()) port = int.Parse(portOption.Value());

                Run(port);

                return 0;
            });
            cmd.Execute(args);
        }

        private static void Run(int port)
        {
            Console.WriteLine("Note: Press <CTRL-C> to stop server.");

            // Create and run application, which does all the real work.
            _listener = new SocketListener(port);

            // Start listening on the specified port, and get callback whenever
            // new socket connection is established.
            _listener.Start(socket =>
            {
                // New connection. Start reading data from the network stream.
                // Socket stream reader will call back when a valid value is read
                // and/or when a terminate command is received.
                var reader = new SocketReader(socket);
                reader.Read(ProcessValue);
            });
            Console.CancelKeyPress += delegate
            {
                StopServer();
            };

            // Block on exit signal to keep process running until exit event encountered
            _exitSignal.Wait();
        }

        private static void ProcessValue(int value)
        {
            Console.WriteLine(value);
        }

        private static void StopServer()
        {
            Console.WriteLine("Stopping server...");
            try
            {
                _listener.Stop();
                _exitSignal.Set();
            }
            catch { }
        }


    }
}
