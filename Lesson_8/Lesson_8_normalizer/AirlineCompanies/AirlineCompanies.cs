using Model;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using Utility;

namespace AirlineCompanies
{
    public class AirlineCompanies
    {
        internal void Run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                SendMessage(Companies.KLM);
                Thread.Sleep(1000);
                SendMessage(Companies.SAS);
                Thread.Sleep(1000);
                SendMessage(Companies.SW);
            }
        }

        private void SendMessage(Companies company)
        {
            byte[] bytes;
            var airlineCompanies = "AirlineCompanies";
            switch (company)
            {
                case Companies.KLM:
                    bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(GenerateKlm()));
                    ChannelFactory.SendMessageOnChannel(airlineCompanies, bytes, Enum.GetName(typeof(Companies), company));
                    break;
                case Companies.SAS:
                    bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(GenerateSas()));
                    ChannelFactory.SendMessageOnChannel(airlineCompanies, bytes, Enum.GetName(typeof(Companies), company));
                    break;
                case Companies.SW:
                    bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(GenerateSw()));
                    ChannelFactory.SendMessageOnChannel(airlineCompanies, bytes, Enum.GetName(typeof(Companies), company));
                    break;
                default:
                    break;
            }
            Console.WriteLine("Message sent for " + Enum.GetName(typeof(Companies), company));
        }

        private AirlineCompanyKLM GenerateKlm()
        {
            return new AirlineCompanyKLM()
            {
                Airline = "KLM",
                FlightNo = "154",
                Destination = "San Diego",
                Origin = "Schipol",
                dateslashtime = DateTime.Now,
            };
        }
        private AirlineCompanySAS GenerateSas()
        {
            return new AirlineCompanySAS()
            {
                Airline = "SAS",
                FlightNo = "SK 239 ",
                Destination = "JFK",
                Origin = "CPH",
                ArivalDeparture = "D",
                dato = DateTime.Now.ToLongDateString(),
                tidspunkt = DateTime.Now.ToLongTimeString()
            };
        }
        private AirlineCompanySW GenerateSw()
        {
            return new AirlineCompanySW()
            {
                Airline = "South West Airlines",
                FlightNo = "SW056",
                Destination = "New York",
                date = DateTime.Now.ToLongDateString(),
                departure = DateTime.Now.ToLongTimeString()
            };
        }
        enum Companies
        {
            KLM,
            SAS,
            SW
        }
    }
}