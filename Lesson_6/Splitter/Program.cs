using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Utility;

namespace Splitter
{
    class Program
    {
        static void Main(string[] args)
        {
            string checkinout = "CheckInOut";
            IModel channel = ChannelFactory.CreateDirectChannel(checkinout);

            try
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (ch, ea) =>
                {
                    var checkInXDocument = ea.Body.ToXDocument();
                    //Validate XSD

                    var checkInCorrelationId = Guid.NewGuid();
                                        
                    string passengerChannelName = "Passenger";
                    var personXDocument = SplitPassengerFromCheckIn(checkInXDocument, checkInCorrelationId);
                    var passengerChannel = ChannelFactory.CreateDirectChannel(passengerChannelName);
                    passengerChannel.BasicPublish(passengerChannelName + "Exchange", passengerChannelName + "RoutingKey", null, personXDocument.ToByteArray());
                    Console.WriteLine(passengerChannelName + " message sent");
                    Console.WriteLine();

                    string luggageChannelName = "Luggage";
                    var luggageXDocuments = SplitLuggageFromCheckIn(checkInXDocument, checkInCorrelationId);
                    var luggageChannel = ChannelFactory.CreateDirectChannel(luggageChannelName);
                    
                    foreach (var luggageXDocument in luggageXDocuments)
                    {
                        luggageChannel.BasicPublish(luggageChannelName + "Exchange", luggageChannelName + "RoutingKey", null, luggageXDocument.ToByteArray());
                        Console.WriteLine(luggageChannelName + " message sent");
                        Console.WriteLine();
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                };
                string consumerTag = channel.BasicConsume(checkinout+"Queue", false, consumer);
            }
            finally
            {
                Console.WriteLine("Press Any key to close the splitter.");
                Console.ReadKey();
                channel.Close();
            }
        }

        private static IEnumerable<XDocument> SplitLuggageFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting luggage with CorrelationId " + checkInCorrelationId);
            Console.WriteLine();
            foreach (var luggageElement in checkInDocument.Root.Elements().Where(e => e.Name == "Luggage"))
            {
                var luggageDocument = new XDocument(checkInDocument);

                luggageDocument.Root.Name = "LuggageFlightDetailsInfoResponse";

                luggageDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
                luggageDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();
                luggageDocument.Root.Add(luggageElement);

                Console.WriteLine(luggageDocument);
                Console.WriteLine();
                yield return luggageDocument;
            }
        }

        private static XDocument SplitPassengerFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting passenger with CorrelationId " + checkInCorrelationId);
            Console.WriteLine();

            var passengerDocument = new XDocument(checkInDocument);
            var totalLuggage = passengerDocument.Root.Elements().Count(e => e.Name == "Luggage");            
            
            passengerDocument.Root.Name = "PassengerFlightDetailsInfoResponse";
            passengerDocument.Root.Add(new XElement("TotalLuggage", totalLuggage));
            passengerDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
            passengerDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();
            
            Console.WriteLine(passengerDocument);
            Console.WriteLine();
            return passengerDocument;
        }
    }
}
