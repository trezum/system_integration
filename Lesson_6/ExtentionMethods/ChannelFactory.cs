using RabbitMQ.Client;
using System;

namespace Utility
{
    public class ChannelFactory
    {
        private static IConnection connection;
        public static IModel CreateDirectChannel(string name)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            if (connection == null || !connection.IsOpen)
            {
                connection = factory.CreateConnection();
                Console.WriteLine("Connected");
                Console.WriteLine();
            }

            // Channels should be reused when possible.
            IModel channel = connection.CreateModel();
            Console.WriteLine("Direct channel "+ name + " open");
            Console.WriteLine();

            var exchangeName = name + "Exchange";
            var queueName = name + "Queue";
            var routingKey = name + "RoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            return channel;
        }
    }
}
