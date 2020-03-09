using System;

namespace CanonicalModel
{
    public enum FlightDirection
    {
        Unknown,
        Arrival,
        Departure
    }

    public class CanonicalFlight
    {
        public string Airline { get; set; } // Full name of airline
        public string FlightNo { get; set; } // ICAO Flight No
        public string Origin { get; set; } // ICAO airport code
        public string Destination { get; set; } // ICAO airport code
        public DateTime EventDateTime { get; set; }
        public FlightDirection Direction { get; set; }
    }
}