using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utility;

namespace Resequencer
{
    public class Resequencer
    {
        private Dictionary<Guid, List<XDocument>> _luggageDocuments;

        public Resequencer()
        {
            _luggageDocuments = new Dictionary<Guid, List<XDocument>>();
        }

        internal void Run()
        {
            string luggageChannelName = "Luggage";
            var luggageChannel = ChannelFactory.CreateDirectChannel(luggageChannelName);

            var consumer = new EventingBasicConsumer(luggageChannel);
            consumer.Received += (ch, ea) =>
            {
                handleLuggageMeassage(ea.Body.ToXDocument());
                luggageChannel.BasicAck(ea.DeliveryTag, false);
            };
            var consumerTag = luggageChannel.BasicConsume(luggageChannelName + "Queue", false, consumer);
        }

        private void handleLuggageMeassage(XDocument luggageDocument)
        {
            var correlationId = Guid.Parse(luggageDocument.Root.Element("CorrelationId").Value);
            if (_luggageDocuments.ContainsKey(correlationId))
            {
                _luggageDocuments[correlationId].Add(luggageDocument);
            }
            else
            {
                _luggageDocuments.Add(correlationId, new List<XDocument>() { luggageDocument });
            }

            SendSequenceIfFull(correlationId);
        }

        private void SendSequenceIfFull(Guid correlationId)
        {
            //wait for all strategy
            var totalLuggage = int.Parse(_luggageDocuments[correlationId].First().Root.Element("TotalLuggage").Value);

            var coll = _luggageDocuments[correlationId];
            if (totalLuggage > coll.Count())
                return;

            string sequencedCheckIn = "SequencedCheckIn";
            IModel channel = ChannelFactory.CreateDirectChannel(sequencedCheckIn);

            var luggageColl = _luggageDocuments[correlationId].OrderBy(d => d.Root.Element("Identification"));
            foreach (var luggageDocument in luggageColl)
            {
                channel.BasicPublish(sequencedCheckIn + "Exchange", sequencedCheckIn + "RoutingKey", null, luggageDocument.ToByteArray());
                Console.WriteLine("Sent luggage message for CorrelationId " + correlationId);
            }

            _luggageDocuments.Remove(correlationId);

            Console.WriteLine("Press any key to continue with the next message.");
            Console.ReadKey();
        }
    }
}