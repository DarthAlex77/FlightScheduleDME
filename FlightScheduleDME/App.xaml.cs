using System.IO;
using System.Windows;
using FlightScheduleDME.View;
using FlightScheduleDME.ViewModel;
using Microsoft.Extensions.Configuration;

namespace FlightScheduleDME
{
    public partial class App
    {
        public static FlightScheduleConfig Settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            IConfigurationRoot    config  = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();
            IConfigurationSection section = config.GetSection(nameof(FlightScheduleConfig));
            Settings = section.Get<FlightScheduleConfig>() ?? throw new InvalidOperationException();
            int departureWindowNumber = 0;
            int arrivalWindowNumber = 0;
            foreach (WindowConfig windowConfig in Settings.WindowConfigs)
            {
                if (windowConfig.IsArrival)
                {
                    ArrivalWindow          arrivalWindow = new ArrivalWindow();
                    ArrivalWindowViewModel vm              = (ArrivalWindowViewModel) arrivalWindow.DataContext;
                    vm.TwoColumnPerWindow = windowConfig.TwoColumnPerWindow;
                    vm.LinesPerTable      = windowConfig.LinesPerTable;
                    vm.WindowNumber       = arrivalWindowNumber;
                    if (windowConfig.TwoColumnPerWindow)
                    {
                        arrivalWindowNumber = + 2;
                    }
                    else
                    {
                        arrivalWindowNumber++;
                    }
                    vm.GetArrivalDetails();
                    arrivalWindow.Show();
                }
                else
                {
                    DepartureWindow          departureWindow = new DepartureWindow();
                    DepartureWindowViewModel vm              = (DepartureWindowViewModel) departureWindow.DataContext;
                    vm.TwoColumnPerWindow = windowConfig.TwoColumnPerWindow;
                    vm.LinesPerTable      = windowConfig.LinesPerTable;
                    vm.WindowNumber       = departureWindowNumber;
                    if (windowConfig.TwoColumnPerWindow)
                    {
                        departureWindowNumber =+ 2;
                    }
                    else
                    {
                        departureWindowNumber++;
                    }
                    vm.GetDepartureDetails();
                    departureWindow.Show();
                }
            }
            base.OnStartup(e);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("Error.log", DateTime.Now + " " + e.ExceptionObject);
            MessageBox.Show("Критическая ошибка. Программа будет закрыта.");
            Current.Shutdown(-1);
        }
    }
}