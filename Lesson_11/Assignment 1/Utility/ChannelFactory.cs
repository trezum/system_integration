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

        public static IModel CreateDirectChannel(string name, string optionalRoutingKey = null, string[] optionalQueueNames = null)
        {
            var exchangeName = name + "Exchange";
            var queueName = name + "Queue";
            var routingKey = string.IsNullOrWhiteSpace(optionalRoutingKey) ? name + "RoutingKey" : optionalRoutingKey;

            var queueNames = "";
            if (optionalQueueNames != null && optionalQueueNames.Length > 0)
            {
                queueNames = string.Concat(optionalQueueNames);
            }

            var channelKey = name + routingKey + queueNames;

            if (_directChannels.ContainsKey(channelKey) && _directChannels[channelKey].IsOpen)
            {
                Console.WriteLine("Reusing channel " + channelKey);
                return _directChannels[channelKey];
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

            if (optionalQueueNames != null && optionalQueueNames.Length > 0)
            {
                foreach (var qName in optionalQueueNames)
                {
                    channel.ExchangeDeclare(qName + "Exchange", ExchangeType.Direct);
                    channel.QueueDeclare(qName + "Queue", false, false, false, null);
                    channel.QueueBind(qName + "Queue", qName + "Exchange", routingKey, null);
                }
            }
            else
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);
            }
            _directChannels.Add(channelKey, channel);

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