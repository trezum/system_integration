using System;
using System.Xml.Linq;
using ExtentionMethods;
using RabbitMQ.Client;

namespace BluffCityCheckInOutputRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672/");
            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();
            Console.WriteLine("Channel open");

            var exchangeName = "CheckInOutputExchange";
            var queueName = "CheckInOutputQueue";
            var routingKey = "CheckInOutputRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);
            
            XDocument CheckInFile = XDocument.Load(@"CheckedInPassenger.xml");
            var bytes = CheckInFile.ToByteArray();

            while (true)
            {                
                channel.BasicPublish(exchangeName, routingKey, null, bytes);
                Console.WriteLine("Message sent.");
                Console.ReadKey();
            }            
        }
    }
}
