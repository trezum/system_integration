using System;
using System.Xml.Linq;
using Utility;
using RabbitMQ.Client;

namespace BluffCityCheckInOutputRabbitMQ
{
    public class CheckInOutput
    {
        internal void Run()
        {
            string checkinout = "CheckInOut";
            var channel = ChannelFactory.CreateDirectChannel(checkinout);

            XDocument CheckInFile = XDocument.Load(@"CheckedInPassenger.xml");
            var bytes = CheckInFile.ToByteArray();

            while (true)
            {
                channel.BasicPublish(checkinout + "Exchange", checkinout + "RoutingKey", null, bytes);
                Console.WriteLine("Message sent on the channel CheckInOut.");
                Console.WriteLine("Press any key to continue with the next message.");
                Console.ReadKey();
            }
        }
    }
}
