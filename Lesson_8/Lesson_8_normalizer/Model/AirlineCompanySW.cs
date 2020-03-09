using System;

namespace Model
{
    public class AirlineCompanySW
    {
        private DateTime EventDateTime;

        public string Airline { get; set; } //South West Airlines
        public string FlightNo { get; set; } //SW056
        public string Destination { get; set; } //New York
        public string date //03/06/2017
        {
            get => EventDateTime.ToLongDateString();
            set => EventDateTime = DateTime.Parse(value + " " + EventDateTime.ToLongTimeString());
        }

        public string departure //09:45 PM
        {
            get => EventDateTime.ToLongTimeString();
            set => EventDateTime = DateTime.Parse(EventDateTime.ToLongDateString() + " " + value);
        }
    }
}