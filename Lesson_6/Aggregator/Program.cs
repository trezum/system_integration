using System;
using Utility;

namespace Aggregator
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;
            new Aggregator().Run();
        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            ChannelFactory.Dispose();
            Console.WriteLine("The process has exited, press any key to close this window.");
            Console.ReadKey();
        }
    }
}