using System;
using System.Xml.Linq;
using Utility;
using RabbitMQ.Client;

namespace BluffCityCheckInOutputRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            string checkinout = "CheckInOut";
            var channel = ChannelFactory.CreateDirectChannel(checkinout);

            XDocument CheckInFile = XDocument.Load(@"CheckedInPassenger.xml");
            var bytes = CheckInFile.ToByteArray();

            while (true)
            {                
                channel.BasicPublish(checkinout + "Exchange", checkinout + "RoutingKey", null, bytes);
                Console.WriteLine("Message sent on the channel CheckInOut.");
                Console.ReadKey();
            }
        }
    }
}