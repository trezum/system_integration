using CanonicalModel;
using Model;
using System;

namespace Translator.Translators
{
    internal class SwTranslator : AbstractTranslator<AirlineCompanySW>
    {
        public override string RoutingKey()
        {
            return "SW";
        }
        public override CanonicalFlight Translate(AirlineCompanySW swObject)
        {
            return new CanonicalFlight()
            {
                Airline = "Southwest Airlines",
                Destination = AirportDatabase.LookupIATACode(swObject.Destination),
                Direction = FlightDirection.Unknown,
                EventDateTime = DateTime.Parse(swObject.date + " " + swObject.departure),
                FlightNo = "SWA" + swObject.FlightNo.Substring(3),
                Origin = "",
            };
        }
    }
}