using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bluff.Router
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Router started!");

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");

            // Use one connection for the whole program if possible, fewer is better.
            IConnection conn = factory.CreateConnection();
            Console.WriteLine("Connected");

            // Channels should be reused when possible.
            IModel channel = conn.CreateModel();
            Console.WriteLine("Channel open");


            // Setting up input queue
            var exchangeName = "MyRoutingExchange";
            var queueName = "MyRoutingQueue";
            var routingKey = "UnusedRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            //byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("This message is sent in the queue");
            //channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);

            //Setting up output queues

            var routingKeys = new[] { "SAS", "SWA", "KLM" };
            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var myRoutingKey = System.Text.Encoding.UTF8.GetString((Byte[])ea.BasicProperties.Headers["MyRoutingKey"]);

                var outputChannel = GetChannelFromKey(myRoutingKey,conn);
                outputChannel.BasicPublish(myRoutingKey + "-RExchange", "UnusedRoutingKey", ea.BasicProperties, ea.Body); 

                Console.WriteLine(System.Text.Encoding.UTF8.GetString(body));
                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Press Any key to close the program.");
            Console.ReadKey();

            channel.Close();
            Console.WriteLine("Channel closed");
            conn.Close();
            Console.WriteLine("Disconnected");
        }

        private static IModel GetChannelFromKey(string key,IConnection conn)
        {
            var routingKey = key+"-R";
            var queueName = routingKey + "Queue";
            var exchangeName = routingKey + "Exchange";

            IModel channel = conn.CreateModel();
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, "UnusedRoutingKey", null);

            return channel;
        }
    }
}
