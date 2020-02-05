using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Bluff.Messages;
using System.Threading;

namespace Bluff.AirlineCompanies.SAS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Airline Company SAS");

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            IConnection conn = factory.CreateConnection();
            Console.WriteLine("Connected");

            // Channels should be reused when possible.
            IModel channel = conn.CreateModel();
            Console.WriteLine("Channel open");


            var exchangeName = "MyFirstExchange";
            var queueName = "MyFirstQueue";
            var routingKey = "MyFirstRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            var plane = new Plane() { Airline = "KLM", FlightNr = "KL1108", Time = "11:25", To = "Amsterdam Schipol (AMS)" };

            var msgBody = JsonSerializer.Serialize(plane);
                        
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(msgBody);

            while (true)
            {
                Thread.Sleep(1000);
                channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
            }
            

            Console.WriteLine("Press Any key to close the program.");
            Console.ReadKey();


            channel.Close();
            Console.WriteLine("Channel closed");
            conn.Close();
            Console.WriteLine("Disconnected");
        }
    }
}
