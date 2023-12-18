using System.Net.Http;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace FlightScheduleDME.Model
{
    public static class DmeParser
    {
        private static List<IElement> DownloadSite(char direction, int hour, int day)
        {
            if (direction != 'A' && direction != 'D')
            {
                throw new Exception("Direction must be A (Arrival) or D (Departure)");
            }
            if (hour < 0 || hour > 23)
            {
                throw new Exception("Hour must be <0 and >23");
            }
            if (day != 0 && day != 1)
            {
                throw new Exception("day must be 0(today) or 1 (tomorrow)");
            }
            HtmlParser     parser   = new HtmlParser();
            HttpClient     client   = new HttpClient();
            bool           parsed   = false;
            List<IElement> elements = new List<IElement>(150);
            int            @try     = 0;
            while (!parsed)
            {
                string html;
                try
                {
                    html = client.GetStringAsync($"https://m.dme.ru/passengers/flight/live-board/?direction={direction}&searchtext=&periodday={day}&periodhour={hour}").Result;
                }
                catch (Exception)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    @try++;
                    if (@try == 5)
                    {
                        throw;
                    }
                    continue;
                }
                IHtmlDocument document = parser.ParseDocument(html);
                elements.AddRange(document.GetElementById("idFlightList").GetElementsByTagName("li"));
                parsed = true;
            }
            return elements;
        }

        private static List<Flight> GetFlight(int hour, char directory, int day)
        {
            List<Flight> flights = new List<Flight>(50);
            foreach (IElement element in DownloadSite(directory, hour, day))
            {
                Flight flight = new Flight
                {
                    Id = "https://m.dme.ru/" + element.Children[0].GetAttribute("href"),
                    ActualTime = string.IsNullOrWhiteSpace(element.GetElementsByClassName("actualDateFlight")[0].TextContent) ? null
                                 : DateTime.Parse(element.GetElementsByClassName("actualDateFlight")[0].TextContent)
                };
                if (day == 0)
                {
                    flight.PlannedTime = string.IsNullOrWhiteSpace(element.GetElementsByClassName("plannedDateFlight")[0].TextContent) ? DateTime.Today
                                         : DateTime.Parse(element.GetElementsByClassName("plannedDateFlight")[0].TextContent);
                }
                else
                {
                    flight.PlannedTime = string.IsNullOrWhiteSpace(element.GetElementsByClassName("plannedDateFlight")[0].TextContent) ? DateTime.Today
                                         : DateTime.Parse(element.GetElementsByClassName("plannedDateFlight")[0].TextContent).AddDays(1);
                }
                string[] fn = element.GetElementsByClassName("flightNumber")[0].Children[0].TextContent.ReplaceLineEndings("").Trim().Split(" ");
                flight.FlightNumber = fn[0] + " " + fn[1];
                flight.FlightStatus = GetFlightStatus(element);
                flight.FlightStatusString = string.IsNullOrWhiteSpace(element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim()) ? "По расписанию"
                                            : element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim();

                flight.City = element.GetElementsByClassName("destinationFlight")[0].TextContent.ReplaceLineEndings("").Trim();

                flights.Add(flight);
            }
            List<Flight> list = flights.DistinctBy(x => x.FlightNumber)           // Удаляем дубликаты рейсов
                                       .GroupBy(x => new {x.PlannedTime, x.City}) //Группировка по времени и городу, чтобы собрать в одни совмещенный рейсы
                                       .Select(g => new Flight
                                       {
                                           ActualTime         = g.First().ActualTime,
                                           PlannedTime        = g.Key.PlannedTime,
                                           FlightNumber       = string.Join(" ", g.Select(t => t.FlightNumber)),
                                           City               = g.Key.City,
                                           FlightStatus       = g.First().FlightStatus,
                                           FlightStatusString = g.First().FlightStatusString,
                                           Id                 = g.First().Id
                                       })
                                       .ToList();

            return list;
        }

        private static FlightStatus GetFlightStatus(IElement element)
        {
            if (element.GetElementsByClassName("flightstatus")[0].GetAttribute("style") == "color:#f08130") //Задерживается 
            {
                if (element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Рейс вылетел")
                {
                    return FlightStatus.Departed;
                }
                if (element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Прибыл" ||
                    element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Приземлился")
                {
                    return FlightStatus.Arrived;
                }
                return FlightStatus.Delayed;
            }
            if (element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Прибыл" ||
                element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Приземлился")
            {
                return FlightStatus.Arrived;
            }
            if (element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Рейс вылетел")
            {
                return FlightStatus.Departed;
            }
            if (element.GetElementsByClassName("flightstatus")[0].TextContent.ReplaceLineEndings("").Trim() == "Рейс отменен")
            {
                return FlightStatus.Cancelled;
            }
            return FlightStatus.OnTime;
        }

        public static List<Arrival> GetAllArrivals()
        {
            List<Arrival> arrivals = new List<Arrival>();
            for (int day = 0; day < 2; day++)
            {
                int hour = 0;
                for (int i = 0; i < 4; i++)
                {
                    arrivals.AddRange(GetFlight(hour, 'A', day)
                                      .DistinctBy(x => x.FlightNumber)
                                      .Select(x => new Arrival
                                      {
                                          City               = x.City,
                                          FlightNumber       = x.FlightNumber,
                                          FlightStatus       = x.FlightStatus,
                                          FlightStatusString = x.FlightStatusString,
                                          Id                 = x.Id,
                                          ActualTime         = x.ActualTime,
                                          PlannedTime        = x.PlannedTime
                                      }));
                    if (hour == 0)
                    {
                        hour += 9;
                    }
                    else
                    {
                        hour += 6;
                    }
                }
            }
            return arrivals.DistinctBy(x => x.FlightNumber).ToList();
        }

        public static string?[] GetArrivalDetail(string id)
        {
            string?[]  details = new string?[5];
            HtmlParser parser  = new HtmlParser();
            HttpClient client  = new HttpClient();
            string     html    = string.Empty;
            int        @try    = 0;
            bool       parsed  = false;
            while (!parsed)
            {
                try
                {
                    html   = client.GetStringAsync(id).Result;
                    parsed = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    @try++;
                    if (@try == 5)
                    {
                        throw;
                    }
                }
            }
            IHtmlDocument document = parser.ParseDocument(html);
            details[0] = document.GetElementById("offsetStart")?.TextContent.Replace(" \n", "").Trim();
            IHtmlCollection<IElement> elements = document.GetElementsByClassName("details")[0].GetElementsByTagName("li");
            string[]                  strings1 = elements[7].TextContent.Replace(" \n", "").Trim().Split('\n');
            details[1] = strings1.Length > 1 ? strings1[1].Trim() : string.Empty;
            return details;
        }

        public static List<Departure> GetAllDepartures()
        {
            List<Departure> departures = new List<Departure>();

            for (int day = 0; day < 2; day++)
            {
                int hour = 0;
                for (int i = 0; i < 4; i++)
                {
                    departures.AddRange(GetFlight(hour, 'D', day)
                                        .Select(x => new Departure
                                        {
                                            City               = x.City,
                                            FlightNumber       = x.FlightNumber,
                                            FlightStatus       = x.FlightStatus,
                                            FlightStatusString = x.FlightStatusString,
                                            Id                 = x.Id,
                                            ActualTime         = x.ActualTime,
                                            PlannedTime        = x.PlannedTime
                                        }));
                    if (hour == 0)
                    {
                        hour += 9;
                    }
                    else
                    {
                        hour += 6;
                    }
                }
            }

            return departures.DistinctBy(x => x.FlightNumber).ToList();
        }

        public static string?[] GetDepartureDetail(string id)
        {
            string?[]  details = new string?[5];
            HtmlParser parser  = new HtmlParser();
            HttpClient client  = new HttpClient();
            string     html    = string.Empty;
            int        @try    = 0;
            bool       parsed  = false;
            while (!parsed)
            {
                try
                {
                    html   = client.GetStringAsync(id).Result;
                    parsed = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    @try++;
                    if (@try == 5)
                    {
                        throw;
                    }
                }
            }
            IHtmlDocument             document = parser.ParseDocument(html);
            IHtmlCollection<IElement> elements = document.GetElementsByClassName("details")[0].GetElementsByTagName("li");
            string[]                  strings1 = elements[4].TextContent.Replace(" \n", "").Trim().Split('\n');
            string[]                  strings2 = elements[7].TextContent.Replace(" \n", "").Trim().Split('\n');
            string[]                  strings3 = elements[8].TextContent.Replace(" \n", "").Trim().Split('\n');
            string[]                  strings4 = elements[9].TextContent.Replace(" \n", "").Trim().Split('\n');
            string[]                  strings5 = elements[10].TextContent.Replace(" \n", "").Trim().Split('\n');
            details[0] = strings1.Length > 1 ? strings1[1].Trim() : string.Empty;
            details[1] = strings2.Length > 1 ? strings2[1].Trim() : string.Empty;
            details[2] = strings3.Length > 1 ? strings3[1].Trim() : string.Empty;
            details[3] = strings4.Length > 1 ? strings4[1].Trim() : string.Empty;
            details[4] = strings5.Length > 1 ? strings5[1].Trim() : string.Empty;
            return details;
        }
    }
}