using CanonicalDataModel;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using Utility;

namespace EUCCID
{
    public class EUCCIDProducer
    {
        public void Run()
        {
            Console.WriteLine("This app generates new pople formatted in the EU-CCID Canonical datamodel");
            Console.WriteLine("and adds them to the queue going from EU-CCID to the translator infront of the CPR system.");

            var channelName = "EUCCIDtoCPR";
            while (true)
            {
                var json = JsonSerializer.Serialize(GenerateEuccidModel());
                ChannelFactory.SendMessageOnChannel(channelName, Encoding.ASCII.GetBytes(json));
                Console.WriteLine("EUCCIDtoCPR message sent, pausing for 2 seconds");
                Thread.Sleep(2000);
            }
        }

        private EuCcidModel GenerateEuccidModel()
        {
            return new EuCcidModel()
            {
                ChristianName = "Clara",
                FamilyName = "Johnson",
                EuCcid = "01121990-123456",
                Gender = Gender.Female,
                StreetPlusNumberOfHouse = "Ravine Road 67",
                ApartmentNumber = "",
                County = "Greater Manchester",
                City = "Salford",
                BirthCountry = "UK",
                CurrentLivingInCountry = "UK",
            };
        }
    }
}
