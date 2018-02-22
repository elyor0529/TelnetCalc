using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var port = 10083;
            
            //Create a service application that runs on 10083 TCP port
            var serviceApplication = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(port));

            //Create a CalculatorService and add it to service application
            serviceApplication.AddService<ICalculatorService, CalculatorService>(new CalculatorService());
            
            //Start service application
            serviceApplication.Start();
            Console.WriteLine($"Calculator service is started by {port}");
            
            //events
            serviceApplication.ClientConnected+=ServiceApplicationOnClientConnected;
            serviceApplication.ClientDisconnected+=ServiceApplicationOnClientDisconnected;
            
            Console.WriteLine("Press enter to stop...");
            Console.ReadLine();

            //Stop service application
            serviceApplication.RemoveService<ICalculatorService>();
            serviceApplication.Stop();
            
        }

        private static void ServiceApplicationOnClientDisconnected(object sender, ServiceClientEventArgs serviceClientEventArgs)
        {
            var serv = serviceClientEventArgs.Client.GetClientProxy<ICalculatorService>();
            serv.Init();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Client#{serviceClientEventArgs.Client.ClientId} is disconnected");
            Console.ResetColor();
        }

        private static void ServiceApplicationOnClientConnected(object sender, ServiceClientEventArgs serviceClientEventArgs)
        {
            var serv = serviceClientEventArgs.Client.GetClientProxy<ICalculatorService>();
            serv.Clear();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Client#{serviceClientEventArgs.Client.ClientId} is connected");
            Console.ResetColor();
        }
    }
}