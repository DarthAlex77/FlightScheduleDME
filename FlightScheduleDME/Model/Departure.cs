namespace FlightScheduleDME.Model
{
    public class Departure : Flight
    {
        public string? RegistrationStatus { get; set; }
        public string? BoardingStatus     { get; set; }
        public string? Gate               { get; set; }
        public string? CheckInDesk        { get; set; }
    }
}