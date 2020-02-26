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
            AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;

            string checkinout = "CheckInOut";
            IModel channel = ChannelFactory.CreateDirectChannel(checkinout);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var checkInXDocument = ea.Body.ToXDocument();

                var checkInCorrelationId = Guid.NewGuid();

                var personXDocument = SplitPassengerFromCheckIn(checkInXDocument, checkInCorrelationId);
                string passengerChannelName = "Passenger";
                var passengerChannel = ChannelFactory.CreateDirectChannel(passengerChannelName);
                passengerChannel.BasicPublish(passengerChannelName + "Exchange", passengerChannelName + "RoutingKey", null, personXDocument.ToByteArray());
                Console.WriteLine("Passenger message sent with CorrelationId: " + checkInCorrelationId);

                var luggageXDocuments = SplitLuggageFromCheckIn(checkInXDocument, checkInCorrelationId);
                string luggageChannelName = "Luggage";
                var luggageChannel = ChannelFactory.CreateDirectChannel(luggageChannelName);
                    
                foreach (var luggageXDocument in luggageXDocuments)
                {
                    luggageChannel.BasicPublish(luggageChannelName + "Exchange", luggageChannelName + "RoutingKey", null, luggageXDocument.ToByteArray());
                    Console.WriteLine("Luggage message sent with CorrelationId: " + checkInCorrelationId);
                }

                channel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            };
            string consumerTag = channel.BasicConsume(checkinout+"Queue", false, consumer);

        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("The process has exited, press any key to close this window.");
            Console.ReadKey();
        }

        private static IEnumerable<XDocument> SplitLuggageFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting luggage with CorrelationId " + checkInCorrelationId);
            foreach (var luggageElement in checkInDocument.Root.Elements().Where(e => e.Name == "Luggage"))
            {
                var luggageDocument = new XDocument(checkInDocument);

                luggageDocument.Root.Name = "LuggageFlightDetailsInfoResponse";
                luggageDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
                luggageDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();
                luggageDocument.Root.Add(luggageElement);

                //Console.WriteLine(luggageDocument);
                Console.WriteLine();
                yield return luggageDocument;
            }
        }

        private static XDocument SplitPassengerFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting passenger with CorrelationId " + checkInCorrelationId);

            var passengerDocument = new XDocument(checkInDocument);
            var totalLuggage = passengerDocument.Root.Elements().Count(e => e.Name == "Luggage");           
            
            passengerDocument.Root.Name = "PassengerFlightDetailsInfoResponse";
            passengerDocument.Root.Add(new XElement("TotalLuggage", totalLuggage));
            passengerDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
            passengerDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();

            return passengerDocument;
        }
    }
}
