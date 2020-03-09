using System;

namespace Model
{
    public class AirlineCompanySAS
    {
        private DateTime EventDateTime;

        public string Airline { get; set; } //SAS 
        public string FlightNo { get; set; } //SK 239 
        public string Destination { get; set; } //JFK 

        public string Origin { get; set; } //CPH 

        public string ArivalDeparture { get; set; } //D

        public string dato //6. marts 2017 
        {
            get => EventDateTime.ToLongDateString();
            set => EventDateTime = DateTime.Parse(value + " " + EventDateTime.ToLongTimeString());
        }

        public string tidspunkt //16:45
        {
            get => EventDateTime.ToLongTimeString();
            set => EventDateTime = DateTime.Parse(EventDateTime.ToLongDateString() + " " + value);
        }
    }
}