namespace FlightScheduleDME.Model
{
    public class Flight
    {
        public string       Id                 { get; set; }
        public DateTime?    ActualTime         { get; set; }
        public DateTime     PlannedTime        { get; set; }
        public string       City               { get; set; }
        public FlightStatus FlightStatus       { get; set; }
        public string?      FlightStatusString { get; set; }
        public string       FlightNumber       { get; set; }
        public TimeSpan TimeToEvent
        {
            get => PlannedTime.Subtract(DateTime.Now);
        }
    }
}