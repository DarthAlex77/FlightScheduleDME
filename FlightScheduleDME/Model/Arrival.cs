namespace FlightScheduleDME.Model
{
    public class Arrival : Flight
    {
        public string   BaggageStatus { get; set; } = null!;
        public TimeOnly DepartureTime { get; set; }
    }
}