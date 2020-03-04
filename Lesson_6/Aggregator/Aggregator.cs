using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RabbitMQ.Client.Events;
using Utility;
using RabbitMQ.Client;

namespace Aggregator
{
    public class Aggregator
    {
        private Dictionary<Guid, List<XDocument>> _luggageDocuments;
        private Dictionary<Guid, XDocument> _passengerDocuments;

        public Aggregator()
        {
            _luggageDocuments = new Dictionary<Guid, List<XDocument>>();
            _passengerDocuments = new Dictionary<Guid, XDocument>();
        }

        internal void Run()
        {
            string sequencedChannelName = "SequencedCheckIn";
            var sequencedChannel = ChannelFactory.CreateDirectChannel(sequencedChannelName);

            var consumer = new EventingBasicConsumer(sequencedChannel);
            consumer.Received += (ch, ea) =>
            {

                var document = ea.Body.ToXDocument();
                AddDocumentToCorrectCollectiono(document);
                PrintAndRemoveDocumentFromMemoryIfComplete(document);
                sequencedChannel.BasicAck(ea.DeliveryTag, false);
            };
            var consumerTag = sequencedChannel.BasicConsume(sequencedChannelName + "Queue", false, consumer);
        }

        private void AddDocumentToCorrectCollectiono(XDocument document)
        {
            var correlationId = Guid.Parse(document.Root.Element("CorrelationId").Value);

            if (document.Root.Name == "PassengerFlightDetailsInfoResponse")
            {
                _passengerDocuments.Add(correlationId, document);
            }

            if (document.Root.Name == "LuggageFlightDetailsInfoResponse")
            {
                if (_luggageDocuments.ContainsKey(correlationId))
                {
                    _luggageDocuments[correlationId].Add(document);
                }
                else
                {
                    _luggageDocuments.Add(correlationId, new List<XDocument>() { document });
                }
            }
        }

        private void PrintAndRemoveDocumentFromMemoryIfComplete(XDocument sequencedDocument)
        {
            var correlationId = Guid.Parse(sequencedDocument.Root.Element("CorrelationId").Value);

            //wait for all strategy
            if (!_passengerDocuments.ContainsKey(correlationId) || !_luggageDocuments.ContainsKey(correlationId))
                return;

            var totalLuggage = int.Parse(_luggageDocuments[correlationId].First().Root.Element("TotalLuggage").Value);

            var coll = _luggageDocuments[correlationId];
            if (totalLuggage > coll.Count())
                return;

            //Recreate the original xml structure

            var recreation = new XDocument(_passengerDocuments[correlationId]);

            recreation.Root.Name = "FlightDetailsInfoResponse";


            recreation.Root.Elements().First(e => e.Name == "CorrelationId").Remove();

            var luggageColl = _luggageDocuments[correlationId].OrderBy(d => d.Root.Element("Identification"));
            foreach (var luggage in luggageColl)
            {
                luggage.Root.Elements().First(e => e.Name == "TotalLuggage").Remove();
                recreation.Root.Add(luggage.Root.Elements().First(e => e.Name == "Luggage"));
            }
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Xml for CorrelationId: " + correlationId);
            Console.WriteLine("-------------------------------");
            Console.WriteLine(recreation.Declaration);
            Console.WriteLine(recreation);
            Console.WriteLine("-------------------------------");

            _luggageDocuments.Remove(correlationId);
            _passengerDocuments.Remove(correlationId);

            Console.WriteLine("Press any key to continue with the next message.");
            Console.ReadKey();
        }
    }
}