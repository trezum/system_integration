using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bluff.AirportInformationCenter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bluff Airport Information Center");

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

            try
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (ch, ea) =>
                {
                    var body = ea.Body;

                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(body));

                    channel.BasicAck(ea.DeliveryTag, false);
                };
                string consumerTag = channel.BasicConsume(queueName, false, consumer);
              
            }
            finally
            {
                Console.WriteLine("Press Any key to close the program.");
                Console.ReadKey();

                channel.Close();
                Console.WriteLine("Channel closed");
                conn.Close();
                Console.WriteLine("Disconnected");
            }
        }
    }
}
