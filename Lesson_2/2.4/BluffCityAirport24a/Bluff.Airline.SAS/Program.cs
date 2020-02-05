﻿using System;
using System.Text.Json;
using Bluff.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bluff.Airline.SAS
{
    class Program
    {
        static void Main(string[] args)
        {            
            var routingKey = "SAS-R";
            var queueName = routingKey + "Queue";
            var exchangeName = routingKey + "Exchange";

            Console.WriteLine(routingKey + " Gate consumer - Routing");

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            IConnection conn = factory.CreateConnection();
            Console.WriteLine("Connected");

            // Channels should be reused when possible.
            IModel channel = conn.CreateModel();
            Console.WriteLine("Channel open");

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, "UnusedRoutingKey", null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var gateUpdate = JsonSerializer.Deserialize<GateUpdate>(body);
                Console.WriteLine(gateUpdate);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Press Any key to close the GateConsumer.");
            Console.ReadKey();

            channel.Close();
            Console.WriteLine("Channel closed");
            conn.Close();
            Console.WriteLine("Disconnected");
        }
    }
}
