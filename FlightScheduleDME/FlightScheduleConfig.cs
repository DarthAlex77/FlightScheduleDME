using FlightScheduleDME.Model;

namespace FlightScheduleDME
{
    public class FlightScheduleConfig
    {
        public TimeSpan           TimeToEventFilterStart { get; set; }
        public TimeSpan           UpdateInterval         { get; set; }
        public List<FlightStatus> StatusesIgnoreFilter   { get; set; } = new List<FlightStatus>();
        public List<WindowConfig> WindowConfigs          { get; set; } = new List<WindowConfig>();
    }
}