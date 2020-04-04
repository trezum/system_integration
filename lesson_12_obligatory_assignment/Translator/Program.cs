﻿using System;
using Utility;

namespace Translator
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;
            new Translator().Run();
        }
        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            ChannelFactory.Dispose();
            Console.WriteLine("The process has exited.");
        }
    }
}
