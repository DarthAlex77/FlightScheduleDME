using System.ComponentModel;

namespace FlightScheduleDME.Model
{
    public enum FlightStatus
    {
        OnTime = 0,
        Arrived = 1,
        Delayed = 2,
        Cancelled = 3,
        Departed = 4
    }
}