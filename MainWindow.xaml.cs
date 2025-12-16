using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Weather.Classes;
using Weather.Elements;
using Weather.Models;

namespace Weather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataResponse response;
        public MainWindow()
        {
            InitializeComponent();
            Iint();
        }

        public async void Iint()
        {
            response = await GetWeather.Get(58.01168f, 56.286672f);

            foreach (Forecast forecast in response.forecasts)
            {
                Days.Items.Add(forecast.date.ToString("dd.MM.yyyy"));
            }
            Create(0);
        }
        public void Create(int idForecast)
        {
            if (response?.forecasts == null || response.forecasts.Count == 0)
            {
                MessageBox.Show("Нет данных о погоде.");
                return;
            }

            if (idForecast < 0 || idForecast >= response.forecasts.Count)
                return;

            parent.Children.Clear();

            foreach (Hour hour in response.forecasts[idForecast].hours)
            {
              
            }
        }


        private void SelectDay(object sender, SelectionChangedEventArgs e)
        {
            if (Days.SelectedIndex >= 0)
                Create(Days.SelectedIndex);
        }


        private void UpdateWeather(object sender, RoutedEventArgs e)
        {
            Iint();

        }

        private void CityBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}