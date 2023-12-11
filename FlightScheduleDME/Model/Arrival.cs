namespace FlightScheduleDME.Model
{
    public class Arrival:Flight
    {
        public string   BaggageStatus { get; set; }
        public TimeOnly DepartureTime { get; set; }
    }
}