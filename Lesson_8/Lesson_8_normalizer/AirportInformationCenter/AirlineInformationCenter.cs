using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Utility;

namespace AirportInformationCenter
{
    internal class AirlineInformationCenter
    {
        internal void Run()
        {
            var canonicalFlight = "CanonicalFlight";
            var canonicalChannel = ChannelFactory.CreateDirectChannel(canonicalFlight);

            var consumer = new EventingBasicConsumer(canonicalChannel);
            consumer.Received += (ch, ea) =>
            {
                Console.WriteLine(Encoding.ASCII.GetString(ea.Body));
                canonicalChannel.BasicAck(ea.DeliveryTag, false);
            };
            var consumerTag = canonicalChannel.BasicConsume(canonicalFlight + "Queue", false, consumer);
        }
    }
}
