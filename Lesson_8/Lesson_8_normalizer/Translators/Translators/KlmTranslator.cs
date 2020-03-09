using CanonicalModel;
using Model;

namespace Translator.Translators
{
    internal class KlmTranslator : AbstractTranslator<AirlineCompanyKLM>
    {
        public override string RoutingKey()
        {
            return "KLM";
        }
        public override CanonicalFlight Translate(AirlineCompanyKLM klmObject)
        {
            return new CanonicalFlight()
            {
                Airline = "Royal Dutch Airlines",
                Destination = AirportDatabase.LookupIATACode(klmObject.Destination),
                Direction = FlightDirection.Unknown,
                EventDateTime = klmObject.dateslashtime,
                FlightNo = "KLM" + klmObject.FlightNo,
                Origin = klmObject.Origin,
            };
        }
    }
}