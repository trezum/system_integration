using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Bluff.Messages;
using RabbitMQ.Client;

namespace Bluff.AirportInformationCenter
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://www.tutlane.com/tutorial/rabbitmq/csharp-rabbitmq-headers-exchange

            Console.WriteLine("Airport Information Center - Routing");

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            IConnection conn = factory.CreateConnection();
            Console.WriteLine("Connected");

            // Channels should be reused when possible.
            IModel channel = conn.CreateModel();
            var properties = channel.CreateBasicProperties();
            properties.Persistent = false;

            Console.WriteLine("Channel open");

            var exchangeName = "MyRoutingExchange";
            var queueName = "MyRoutingQueue";
            var routingKey = "UnusedRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);
            
            var companies = new[] { "SAS", "SWA", "KLM" };
            var random = new Random();

            while (true)
            {
                foreach (var company in companies)
                {
                    var gateUpdate = new GateUpdate()
                    {
                        FlightNr = company + random.Next(1000, 9999),
                        Gate = random.Next(1, 50).ToString()
                    };

                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    dictionary.Add("MyRoutingKey", company);
                    properties.Headers = dictionary;

                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(gateUpdate));
                    channel.BasicPublish(exchangeName, routingKey, properties, messageBodyBytes);
                    Console.WriteLine(gateUpdate.ToString());
                    Thread.Sleep(500);
                }
            }
        }
    }
}
