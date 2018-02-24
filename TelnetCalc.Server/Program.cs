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
        private static SocketServer _server;
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

            _server = new SocketServer(port);
            _server.Start();

            Console.CancelKeyPress += delegate
            {
                Stop();
            };

            _exitSignal.Wait();
        }

        private static void Stop()
        {
            Console.WriteLine("Stopping server...");

            try
            {
                _server.Stop();
                _exitSignal.Set();
            }
            catch { }
        }


    }
}
