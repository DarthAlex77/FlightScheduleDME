using System.Collections.ObjectModel;
using FlightScheduleDME.Model;
using MvvmHelpers;
using Timer = System.Timers.Timer;

namespace FlightScheduleDME.ViewModel
{
    public class ArrivalWindowViewModel : ObservableObject
    {
        public ArrivalWindowViewModel()
        {
            CurrentDateTime = DateTime.Now;
            Timer timeUpdater          = new Timer(TimeSpan.FromSeconds(5));
            Timer allDeparturesUpdater = new Timer(TimeSpan.FromMinutes(10));
            Timer detailsUpdater       = new Timer(App.Settings.UpdateInterval);
            timeUpdater.Elapsed          += (_, _) => CurrentDateTime = DateTime.Now;
            detailsUpdater.Elapsed       += (_, _) => GetArrivalDetails();
            allDeparturesUpdater.Elapsed += (_, _) => SetAllArrivals();
            allDeparturesUpdater.Enabled =  true;
            detailsUpdater.Enabled       =  true;
            timeUpdater.Enabled          =  true;
        }

        #region Methods

        private void SetAllArrivals()
        {
            AllArrivals = DmeParser.GetAllArrivals();
        }

        public void GetArrivalDetails()
        {
            if (AllArrivals == null || !AllArrivals.Any())
            {
                SetAllArrivals();
            }
            List<Arrival> filtered = AllArrivals.Where(flight => flight.TimeToEvent >= App.Settings.TimeToEventFilterStart || App.Settings.StatusesIgnoreFilter.Any(x => flight.FlightStatus == x))
                                                .ToList();
            if (TwoColumnPerWindow)
            {
                Arrivals1 = new ObservableCollection<Arrival>(filtered.Skip(WindowNumber * LinesPerTable).Take(LinesPerTable));
                Arrivals2 = new ObservableCollection<Arrival>(filtered.Skip(WindowNumber + 1 * LinesPerTable).Take(LinesPerTable));
                GetDetails(Arrivals1);
                GetDetails(Arrivals2);
            }
            else
            {
                Arrivals1 = new ObservableCollection<Arrival>(filtered.Skip(WindowNumber * LinesPerTable).Take(LinesPerTable));
                GetDetails(Arrivals1);
            }
            return;

            void GetDetails(IEnumerable<Arrival> arrivals)
            {
                foreach (Arrival arrival in arrivals)
                {
                    string?[] details = DmeParser.GetArrivalDetail(arrival.Id);
                    arrival.BaggageStatus = details[1];
                    arrival.DepartureTime = TimeOnly.Parse(details[0]);
                }
            }
        }

        #endregion

        #region Properties

        private List<Arrival> AllArrivals   { get; set; }
        public  int           LinesPerTable { get; set; }
        public  int           WindowNumber  { get; set; }

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
        public ObservableCollection<Arrival> Arrivals1
        {
            get => _arrivals1;
            set
            {
                if (Equals(value, _arrivals1))
                {
                    return;
                }
                _arrivals1 = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Arrival> Arrivals2
        {
            get => _arrivals2;
            set
            {
                if (Equals(value, _arrivals2))
                {
                    return;
                }
                _arrivals2 = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Fields

        private DateTime                      _currentDateTime;
        private bool                          _twoColumnPerWindow;
        private ObservableCollection<Arrival> _arrivals1;
        private ObservableCollection<Arrival> _arrivals2;

        #endregion
    }
}