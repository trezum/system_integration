using CprDataModel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Utility;

namespace CprSystem
{
    internal class CprConsumer
    {
        private static List<CprModel> database = new List<CprModel>();

        internal void Run()
        {
            var channelNameCPR = "CPR";
            IModel channel = ChannelFactory.CreateDirectChannel(channelNameCPR);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (ch, ea) =>
            {
                var cprModel = JsonSerializer.Deserialize<CprModel>(ea.Body);
                SaveModelToDbForDataEnrichmentBeforeImport(cprModel);
                Console.WriteLine("Message saved to database.");
                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(channelNameCPR + "Queue", false, consumer);
        }

        private void SaveModelToDbForDataEnrichmentBeforeImport(CprModel cprModel)
        {
            database.Add(cprModel);
        }
    }
}