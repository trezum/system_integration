using System;

namespace Translator.Translators
{
    public static class AirportDatabase
    {
        public static string LookupIATACode(string fullname)
        {
            switch (fullname)
            {
                case "San Diego":
                    return "SAN";
                case "New York":
                    return "JFK ";
                case "Schipol":
                    return "AMS";
                default:
                    throw new Exception("Airport not found.");
            }
        }
    }
}
