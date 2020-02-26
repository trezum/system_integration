using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Utility
{
    public class ChannelFactory
    {
        private static IConnection _connection;
        private static Dictionary<string, IModel> _directChannels = new Dictionary<string, IModel>();
        public static IModel CreateDirectChannel(string name)
        {
            if (_directChannels.ContainsKey(name) && _directChannels[name].IsOpen)
            {
                Console.WriteLine("Reusing channel " + name);
                return _directChannels[name];
            }                
            
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = factory.CreateConnection();
                Console.WriteLine("Connected.");
            }

            // Channels should be reused when possible.
            IModel channel = _connection.CreateModel();
            Console.WriteLine("New direct channel "+ name + " opened.");

            var exchangeName = name + "Exchange";
            var queueName = name + "Queue";
            var routingKey = name + "RoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            _directChannels.Add(name,channel);

            return channel;
        }

        public static void Dispose()
        {
            foreach (var channel in _directChannels.Values)
            {                
                channel.Close();
            }
            _connection.Dispose();
        }
    }
}