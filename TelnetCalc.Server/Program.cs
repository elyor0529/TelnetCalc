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
             
            _server.Start(socket =>
            {
                var buffer = new byte[MainSettings.ChunkSize];
                int bytesRead;

                while ((bytesRead = ReadChunk(socket, buffer)) == MainSettings.ChunkSize)
                {
                    if (!ToInt32(buffer, out int value))
                    {
                        break;
                    }

                    ReadValue(value);
                }
            });
            Console.CancelKeyPress += delegate
            {
                Stop();
            };
             
            _exitSignal.Wait();
        } 

        private static int ReadChunk(Socket socket, byte[] buffer)
        { 
            int bytesRead;
            int bufferOffset = 0;

            while (bufferOffset < buffer.Length)
            {
                bytesRead = socket.Receive(buffer, bufferOffset, buffer.Length - bufferOffset, SocketFlags.None);

                if (bytesRead == 0)
                {
                    break;
                }

                bufferOffset += bytesRead;
            }

            return bufferOffset;
        }

        private static bool ToInt32(byte[] buffer, out int value)
        {
            value = 0; 

            for (var i = 0; i < MainSettings.NewLineSize; i++)
            {
                if (buffer[MainSettings.DataSize + i] != MainSettings.NewLineSequence[i])
                {
                    return false;
                }
            } 

            byte b;
            int place;

            for (var i = 0; i < MainSettings.DataSize; i++)
            {
                b = buffer[i];

                if (b < 48 || b > 57)
                {
                    return false;
                }

                place = (int)Math.Pow(10, MainSettings.DataSize - i - 1);
                value += ((b - 48) * place);
            }

            return true;
        }

        private static void ReadValue(int value)
        {
            Console.WriteLine(value);
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
