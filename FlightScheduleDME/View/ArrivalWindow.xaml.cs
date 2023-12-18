using System.Windows;
using System.Windows.Input;

namespace FlightScheduleDME.View
{
    /// <summary>
    ///     Interaction logic for ArrivalWindow.xaml
    /// </summary>
    public partial class ArrivalWindow
    {
        public ArrivalWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Visibility  = Visibility.Collapsed;
                Topmost     = true;
                WindowStyle = WindowStyle.None;
                ResizeMode  = ResizeMode.NoResize;
                Visibility  = Visibility.Visible;
            }
            else
            {
                Topmost     = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode  = ResizeMode.CanResize;
            }
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Topmost     = false;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                Topmost     = true;
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
        }
    }
}