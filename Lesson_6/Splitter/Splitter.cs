﻿using RabbitMQ.Client;
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
                var checkInXDocument = ea.Body.ToXDocument();

                var checkInCorrelationId = Guid.NewGuid();

                var personXDocument = SplitPassengerFromCheckIn(checkInXDocument, checkInCorrelationId);
                string sequencedCheckIn = "SequencedCheckIn";
                var passengerChannel = ChannelFactory.CreateDirectChannel(sequencedCheckIn);
                passengerChannel.BasicPublish(sequencedCheckIn + "Exchange", sequencedCheckIn + "RoutingKey", null, personXDocument.ToByteArray());
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
