using CanonicalModel;
using Model;
using System;

namespace Translator.Translators
{
    internal class SasTranslator : AbstractTranslator<AirlineCompanySAS>
    {
        public override string RoutingKey()
        {
            return "SAS";
        }
        public override CanonicalFlight Translate(AirlineCompanySAS sasObject)
        {
            FlightDirection direction = FlightDirection.Unknown;
            if (sasObject.ArivalDeparture == "A")
            {
                direction = FlightDirection.Arrival;
            }
            else if (sasObject.ArivalDeparture == "D")
            {
                direction = FlightDirection.Departure;
            }

            return new CanonicalFlight()
            {
                Airline = "Scandinavian Airlines",
                Destination = sasObject.Destination,
                Direction = direction,
                EventDateTime = DateTime.Parse(sasObject.dato + " " + sasObject.tidspunkt),
                FlightNo = "SAS" + sasObject.FlightNo.Substring(4),
                Origin = sasObject.Origin,
            };
        }
    }
}