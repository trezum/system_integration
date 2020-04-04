using CanonicalDataModel;
using CprDataModel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using Utility;

namespace Translator
{
    public class Translator
    {
        public void Run()
        {
            var channelNameEU = "EUCCIDtoCPR";
            var channelNameCPR = "CPR";
            IModel channel = ChannelFactory.CreateDirectChannel(channelNameEU);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (ch, ea) =>
            {
                var euccidmodel = JsonSerializer.Deserialize<CanonicalDataModel.EuCcidModel>(ea.Body);
                var json = JsonSerializer.Serialize(TranslateToCpr(euccidmodel));
                ChannelFactory.SendMessageOnChannel(channelNameCPR, Encoding.ASCII.GetBytes(json));
                Console.WriteLine("Message translated and passed on.");
                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(channelNameEU + "Queue", false, consumer);
        }

        private CprModel TranslateToCpr(CanonicalDataModel.EuCcidModel source)
        {
            return new CprModel()
            {
                Fornavn = source.ChristianName,
                Efternavn = source.ChristianName,
                By = source.City,
                Adresse1 = source.StreetPlusNumberOfHouse,
                Adresse2 = source.ApartmentNumber,
                Cprnr = source.EuCcid.Substring(0, 4) + source.EuCcid.Substring(6, 2) + "-XXX" + (source.Gender == Gender.Female ? "2" : "1"),
            };
        }
    }
}
