using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Utility;

namespace Splitter
{
    public class Splitter
    {
        internal void Run()
        {
            string checkinout = "CheckInOut";
            IModel channel = ChannelFactory.CreateDirectChannel(checkinout);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (ch, ea) =>
            {

                //Preparing documents
                var checkInXDocument = ea.Body.ToXDocument();
                var checkInCorrelationId = Guid.NewGuid();
                var personXDocument = SplitPassengerFromCheckIn(checkInXDocument, checkInCorrelationId);
                var luggageXDocuments = SplitLuggageFromCheckIn(checkInXDocument, checkInCorrelationId);

                //Setting up channel
                string combinedChannelName = "Luggage";
                var strings = new string[] { "Luggage", "Passenger" };
                var combinedChannel = ChannelFactory.CreateDirectChannel(combinedChannelName, null, strings);
                combinedChannel.TxSelect();

                //Publishing                
                combinedChannel.BasicPublish("Passenger" + "Exchange", combinedChannelName + "RoutingKey", null, personXDocument.ToByteArray());
                Console.WriteLine("Passenger message sent with CorrelationId: " + checkInCorrelationId);

                //Simulate exception after part of the messages have been sent.
                //throw new Exception();

                foreach (var luggageXDocument in luggageXDocuments)
                {

                    combinedChannel.BasicPublish("Luggage" + "Exchange", combinedChannelName + "RoutingKey", null, luggageXDocument.ToByteArray());
                    Console.WriteLine("Luggage message sent with CorrelationId: " + checkInCorrelationId);
                }

                //Simulate exception after all messages have been sent.
                //throw new Exception();
                combinedChannel.TxCommit();
                channel.BasicAck(ea.DeliveryTag, false);

                Console.WriteLine("Press any key to continue with the next message.");
                Console.WriteLine();
                Console.ReadKey();
            };

            string consumerTag = channel.BasicConsume(checkinout + "Queue", false, consumer);
        }

        private IEnumerable<XDocument> SplitLuggageFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting luggage with CorrelationId " + checkInCorrelationId);
            var totalLuggage = checkInDocument.Root.Elements().Count(e => e.Name == "Luggage");

            foreach (var luggageElement in checkInDocument.Root.Elements().Where(e => e.Name == "Luggage"))
            {
                var luggageDocument = new XDocument(checkInDocument);
                luggageDocument.Root.Name = "LuggageFlightDetailsInfoResponse";
                luggageDocument.Root.Add(new XElement("TotalLuggage", totalLuggage));
                luggageDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
                luggageDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();
                luggageDocument.Root.Add(luggageElement);
                yield return luggageDocument;
            }
        }

        private XDocument SplitPassengerFromCheckIn(XDocument checkInDocument, Guid checkInCorrelationId)
        {
            Console.WriteLine("Splitting passenger with CorrelationId " + checkInCorrelationId);

            var passengerDocument = new XDocument(checkInDocument);
            var totalLuggage = checkInDocument.Root.Elements().Count(e => e.Name == "Luggage");

            passengerDocument.Root.Name = "PassengerFlightDetailsInfoResponse";
            passengerDocument.Root.Add(new XElement("CorrelationId", checkInCorrelationId));
            passengerDocument.Root.Elements().Where(e => e.Name == "Luggage").Remove();

            return passengerDocument;
        }
    }
}
