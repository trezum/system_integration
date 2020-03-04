using System;
using Utility;

namespace BluffCityCheckInOutputRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;
            new CheckInOutput().Run();
        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            ChannelFactory.Dispose();
            Console.WriteLine("The process has exited.");
        }
    }
}