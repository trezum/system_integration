namespace Bluff.Messages
{
    public class GateUpdate
    {
        public string FlightNr { get; set; }
        public string Gate { get; set; }

        public override string ToString()
        {
            return "FlightNr: " + FlightNr + " Gate: " + Gate;
        }
    }
}