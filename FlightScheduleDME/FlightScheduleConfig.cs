using FlightScheduleDME.Model;

namespace FlightScheduleDME
{
    public class FlightScheduleConfig
    {
        public FlightScheduleConfig()
        {
            StatusesIgnoreFilter = new List<FlightStatus>();
            WindowConfigs        = new List<WindowConfig>();
        }

        public TimeSpan           TimeToEventFilterStart { get; set; }
        public TimeSpan           UpdateInterval         { get; set; }
        public List<FlightStatus> StatusesIgnoreFilter   { get; set; }
        public List<WindowConfig> WindowConfigs          { get; set; }
    }
}