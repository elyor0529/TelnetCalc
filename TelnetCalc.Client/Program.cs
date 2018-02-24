using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TelnetCalc.Common;

namespace TelnetCalc.Client
{
    internal class Program
    {
        private static SocketClient _client;

        public static int Main(string[] args)
        {
            var port = 11000;
            var ip = "127.0.0.1";
            var cmd = new CommandLineApplication()
            {
                FullName = "Client Socket Application.",
                Name = "dotnet run --"
            };
            var ipOption = cmd.Option("-ip|--ip <ip>", $"Ip. Default: {ip}", CommandOptionType.SingleValue);
            var portOption = cmd.Option("-port|--port <port>", $"Port. Default: {port}", CommandOptionType.SingleValue);

            cmd.HelpOption("-help|--help");
            cmd.OnExecute(() =>
            {
                if (portOption.HasValue())
                {
                    int.TryParse(portOption.Value(), out port);
                }
                if (ipOption.HasValue())
                {
                    if (IPAddress.TryParse(ipOption.Value(), out var ipAddress))
                    {
                        ip = ipOption.Value();
                    }
                }

                return Run(ip, port);
            });

            return cmd.Execute(args);
        }

        private static int Run(string ip, int port)
        {
            _client = new SocketClient(ip, port);

            if (!_client.Connect())
            {
                return 1;
            }

            _client.Send();
            _client.Disconnect();

            return 0;
        }

    }
}
