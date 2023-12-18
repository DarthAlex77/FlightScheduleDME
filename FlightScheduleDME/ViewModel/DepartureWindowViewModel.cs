using System.Collections.ObjectModel;
using FlightScheduleDME.Model;
using MvvmHelpers;
using Timer = System.Timers.Timer;

namespace FlightScheduleDME.ViewModel
{
    public class DepartureWindowViewModel : ObservableObject
    {
        public DepartureWindowViewModel()
        {
            CurrentDateTime = DateTime.Now;
            Timer timeUpdater          = new Timer(TimeSpan.FromSeconds(5));
            Timer allDeparturesUpdater = new Timer(TimeSpan.FromMinutes(10));
            Timer detailsUpdater       = new Timer(App.Settings.UpdateInterval);
            timeUpdater.Elapsed          += (_, _) => CurrentDateTime = DateTime.Now;
            detailsUpdater.Elapsed       += (_, _) => GetDepartureDetails();
            allDeparturesUpdater.Elapsed += (_, _) => SetAllDepartures();
            allDeparturesUpdater.Enabled =  true;
            detailsUpdater.Enabled       =  true;
            timeUpdater.Enabled          =  true;
        }

        #region Methods

        private void SetAllDepartures()
        {
            AllDepartures = DmeParser.GetAllDepartures();
        }

        public void GetDepartureDetails()
        {
            if (AllDepartures == null || !AllDepartures.Any())
            {
                SetAllDepartures();
            }
            List<Departure> filtered = AllDepartures.Where(flight => flight.TimeToEvent >= App.Settings.TimeToEventFilterStart || App.Settings.StatusesIgnoreFilter.Any(x => flight.FlightStatus == x))
                                                    .ToList();
            if (TwoColumnPerWindow)
            {
                Departures1 = new ObservableCollection<Departure>(filtered.Skip(WindowNumber * LinesPerTable).Take(LinesPerTable));
                Departures2 = new ObservableCollection<Departure>(filtered.Skip(WindowNumber + 1 * LinesPerTable).Take(LinesPerTable));
                GetDetails(Departures1);
                GetDetails(Departures2);
            }
            else
            {
                Departures1 = new ObservableCollection<Departure>(filtered.Skip(WindowNumber * LinesPerTable).Take(LinesPerTable));
                GetDetails(Departures1);
            }
            return;

            void GetDetails(IEnumerable<Departure> departures)
            {
                foreach (Departure departure in departures)
                {
                    string?[] details = DmeParser.GetDepartureDetail(departure.Id);
                    if (!string.IsNullOrWhiteSpace(details[0]))
                    {
                        departure.FlightStatusString = details[0];
                    }
                    departure.RegistrationStatus = details[1];
                    departure.CheckInDesk        = details[2];
                    departure.BoardingStatus     = details[3];
                    departure.Gate               = details[4];
                }
            }
        }

        #endregion

        #region Properties

        private List<Departure>? AllDepartures { get; set; }
        public  int              LinesPerTable { get; set; }
        public  int              WindowNumber  { get; set; }

        #endregion

        #region Bindable Properties

        public bool TwoColumnPerWindow
        {
            get => _twoColumnPerWindow;
            set
            {
                if (value == _twoColumnPerWindow)
                {
                    return;
                }
                _twoColumnPerWindow = value;
                OnPropertyChanged();
            }
        }
        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (value.Equals(_currentDateTime))
                {
                    return;
                }
                _currentDateTime = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Departure> Departures1
        {
            get => _departures1;
            set
            {
                if (Equals(value, _departures1))
                {
                    return;
                }
                _departures1 = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Departure> Departures2
        {
            get => _departures2;
            set
            {
                if (Equals(value, _departures2))
                {
                    return;
                }
                _departures2 = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Fields

        private DateTime                        _currentDateTime;
        private bool                            _twoColumnPerWindow;
        private ObservableCollection<Departure> _departures1;
        private ObservableCollection<Departure> _departures2;

        #endregion
    }
}