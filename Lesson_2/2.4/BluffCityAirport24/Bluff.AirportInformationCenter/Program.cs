using System;
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
            //https://www.tutlane.com/tutorial/rabbitmq/csharp-rabbitmq-direct-exchange
            
            Console.WriteLine("Airport Information Center");

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            IConnection conn = factory.CreateConnection();
            Console.WriteLine("Connected");

            // Channels should be reused when possible.
            IModel channel = conn.CreateModel();
            Console.WriteLine("Channel open");


            var exchangeName = "MyFirstExchange";            
            var routingKeys = new[] { "SAS", "SWA", "KLM" };

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

            foreach (var key in routingKeys)
            {
                var queueName = key+"queue";

                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, key, null);
            }

            var random = new Random();

            while (true)
            {
                foreach (var routingKey in routingKeys)
                {
                    var gateUpdate = new GateUpdate()
                    {
                        FlightNr = routingKey + random.Next(1000, 9999),
                        Gate = random.Next(1, 50).ToString()
                    };

                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(gateUpdate));
                    channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
                    Console.WriteLine(gateUpdate.ToString());
                    Thread.Sleep(500);
                }
            }
        }
    }
}
