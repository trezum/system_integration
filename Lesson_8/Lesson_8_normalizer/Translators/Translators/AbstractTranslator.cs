using CanonicalModel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using Utility;

namespace Translator.Translators
{
    internal abstract class AbstractTranslator<T>
    {
        public abstract string RoutingKey();

        public abstract CanonicalFlight Translate(T sasObject);

        internal void Setup()
        {
            var airlineCompanies = "AirlineCompanies";
            var abstractChannel = ChannelFactory.CreateDirectChannel(airlineCompanies, RoutingKey());
            var consumer = new EventingBasicConsumer(abstractChannel);
            consumer.Received += (ch, ea) =>
            {
                var sasObject = JsonSerializer.Deserialize<T>(Encoding.ASCII.GetString(ea.Body));

                var canonicalFlight = "CanonicalFlight";
                var canonicalChannel = ChannelFactory.CreateDirectChannel(canonicalFlight);

                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                canonicalChannel.BasicPublish(canonicalFlight + "Exchange", canonicalFlight + "RoutingKey", null, Encoding.ASCII.GetBytes(JsonSerializer.Serialize(Translate(sasObject), options)));

                abstractChannel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine("Message from " + RoutingKey() + " route translated and passed on.");
            };
            var consumerTag = abstractChannel.BasicConsume(airlineCompanies + "Queue", false, consumer);
        }
    }
}