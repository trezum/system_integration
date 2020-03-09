using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Utility
{
    public static class ChannelFactory
    {
        private static IConnection _connection;
        private static Dictionary<string, IModel> _directChannels = new Dictionary<string, IModel>();

        public static void SendMessageOnChannel(string channelName, byte[] messageBody, string optionalRoutingKey = null)
        {
            var channel = CreateDirectChannel(channelName, optionalRoutingKey);
            channel.BasicPublish(channelName + "Exchange", optionalRoutingKey, null, messageBody);
        }
        public static IModel CreateDirectChannel(string name, string optionalRoutingKey = null)
        {
            var exchangeName = name + "Exchange";
            var queueName = name + "Queue";
            var routingKey = string.IsNullOrWhiteSpace(optionalRoutingKey) ? name + "RoutingKey" : optionalRoutingKey;

            if (_directChannels.ContainsKey(name + routingKey) && _directChannels[name + routingKey].IsOpen)
            {
                Console.WriteLine("Reusing channel " + name + routingKey);
                return _directChannels[name + routingKey];
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
            Console.WriteLine("New direct channel " + name + " opened.");

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            _directChannels.Add(name + routingKey, channel);

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