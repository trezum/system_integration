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
        private Dictionary<Guid, XDocument> _passengerDocuments;

        public Resequencer()
        {
            _luggageDocuments = new Dictionary<Guid, List<XDocument>>();
            _passengerDocuments = new Dictionary<Guid, XDocument>();
        }

        public void Run()
        {
            string luggageChannelName = "Luggage";
            var luggageChannel = ChannelFactory.CreateDirectChannel(luggageChannelName);
            string passengerChannelName = "Passenger";
            var passengerChannel = ChannelFactory.CreateDirectChannel(passengerChannelName);

            var consumer = new EventingBasicConsumer(luggageChannel);
            consumer.Received += (ch, ea) =>
            {
                handleLuggageMeassage(ea.Body.ToXDocument());
                luggageChannel.BasicAck(ea.DeliveryTag, false);
            };
            var consumerTag = luggageChannel.BasicConsume(luggageChannelName + "Queue", false, consumer);

            var consumer2 = new EventingBasicConsumer(passengerChannel);
            consumer2.Received += (ch, ea) =>
            {
                handlePassengerMeassage(ea.Body.ToXDocument());
                passengerChannel.BasicAck(ea.DeliveryTag, false);
            };
            var consumerTag2 = passengerChannel.BasicConsume(passengerChannelName + "Queue", false, consumer2);
        }

        private void handlePassengerMeassage(XDocument passengerDocument)
        {
            var correlationId = Guid.Parse(passengerDocument.Root.Element("CorrelationId").Value);
            _passengerDocuments.Add(correlationId, passengerDocument);

            SendSequenceIfFull(correlationId);
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
                _luggageDocuments.Add(correlationId, new List<XDocument>() { luggageDocument } );
            }

            SendSequenceIfFull(correlationId);
        }

        private void SendSequenceIfFull(Guid correlationId)
        {
            //wait for all strategy
            if (!_passengerDocuments.ContainsKey(correlationId) || !_luggageDocuments.ContainsKey(correlationId))
                return;

            var totalLuggage = int.Parse(_passengerDocuments[correlationId].Root.Element("TotalLuggage").Value);

            var coll = _luggageDocuments[correlationId];
            if (totalLuggage > coll.Count())
                return;
            
            string sequencedCheckIn = "SequencedCheckIn";
            IModel channel = ChannelFactory.CreateDirectChannel(sequencedCheckIn);
            channel.BasicPublish(sequencedCheckIn + "Exchange", sequencedCheckIn + "RoutingKey", null, _passengerDocuments[correlationId].ToByteArray());
            Console.WriteLine("Sent passenger message for CorrelationId" + correlationId);

            var luggageColl = _luggageDocuments[correlationId].OrderBy(d => d.Root.Element("Identification"));
            foreach (var luggageDocument in luggageColl)
            {                
                channel.BasicPublish(sequencedCheckIn + "Exchange", sequencedCheckIn + "RoutingKey", null, luggageDocument.ToByteArray());
                Console.WriteLine("Sent luggage message for CorrelationId" + correlationId);
            }
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}