using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Weather.Classes;
using Weather.Elements;
using Weather.Models;

namespace Weather
{
    public partial class MainWindow : Window
    {
        private DataResponse? response; // делаем nullable
        private Dictionary<int, string> timeOfDayNames = new Dictionary<int, string>
        {
            { 0, "Ночью" },
            { 6, "Утром" },
            { 12, "Днём" },
            { 18, "Вечером" }
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                response = await WeatherService.GetWeatherCached("Пермь");
                UpdateWeatherDisplay(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateWeatherDisplay(int dayIndex)
        {
            if (response?.forecasts == null || response.forecasts.Count == 0 || dayIndex >= response.forecasts.Count)
            {
                MessageBox.Show("Нет данных для отображения");
                return;
            }

            var forecast = response.forecasts[dayIndex];

            // Обновляем основную информацию
            CurrentDate.Text = forecast.date.ToString("d MMMM, dddd").ToLower();

            // Рассчитываем средние значения для дня
            var allHours = forecast.hours;
            if (allHours.Count > 0)
            {
                var avgTemp = (int)allHours.Average(h => h.temp);
                var minTemp = allHours.Min(h => h.temp);
                var maxTemp = allHours.Max(h => h.temp);
                var avgPressure = (int)allHours.Average(h => h.pressure);
                var avgHumidity = (int)allHours.Average(h => h.humidity);
                var avgWindSpeed = Math.Round(allHours.Average(h => h.wind_speed), 1);
                var avgFeelsLike = (int)allHours.Average(h => h.feels_like);

                // Получаем наиболее частое состояние
                var mostCommonCondition = allHours.GroupBy(h => h.condition)
                                                 .OrderByDescending(g => g.Count())
                                                 .First()
                                                 .First();

                CurrentTemp.Text = $"+{avgTemp}°";
                WeatherDescription.Text = mostCommonCondition.ToCondition();
                TempRange.Text = $"+{minTemp}°...+{maxTemp}°";
                PressureValue.Text = avgPressure.ToString();
                HumidityValue.Text = $"{avgHumidity}%";
                WindValue.Text = $"{avgWindSpeed} {mostCommonCondition.GetWindDirection()}";
                FeelsLikeValue.Text = $"+{avgFeelsLike}°";
            }

            UpdateTimeForecastPanel(forecast);
        }

        private void UpdateTimeForecastPanel(Forecast forecast)
        {
            TimeForecastPanel.Children.Clear();

            // Группируем часы по времени суток
            var timeGroups = new Dictionary<string, List<Hour>>();

            foreach (var hour in forecast.hours)
            {
                int hourInt = int.Parse(hour.hour);
                string timeOfDay = GetTimeOfDay(hourInt);

                if (!timeGroups.ContainsKey(timeOfDay))
                    timeGroups[timeOfDay] = new List<Hour>();

                timeGroups[timeOfDay].Add(hour);
            }

            // Создаем элементы для каждого времени суток
            foreach (var group in timeGroups.OrderBy(g => GetTimeOfDayOrder(g.Key)))
            {
                var timeOfDay = group.Key;
                var hours = group.Value;

                if (hours.Count > 0)
                {
                    TimeForecastPanel.Children.Add(new Items(timeOfDay, hours));
                }
            }
        }

        private int GetTimeOfDayOrder(string timeOfDay)
        {
            return timeOfDay switch
            {
                "Утром" => 1,
                "Днём" => 2,
                "Вечером" => 3,
                "Ночью" => 4,
                _ => 5
            };
        }

        private string GetTimeOfDay(int hour)
        {
            if (hour >= 0 && hour < 6) return "Ночью";
            if (hour >= 6 && hour < 12) return "Утром";
            if (hour >= 12 && hour < 18) return "Днём";
            return "Вечером";
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await FindCityWeather();
        }

        private async Task FindCityWeather()
        {
            if (string.IsNullOrWhiteSpace(CityBox.Text) || CityBox.Text == "Введите город...")
            {
                MessageBox.Show("Введите город!");
                return;
            }

            try
            {
                response = await WeatherService.GetWeatherCached(CityBox.Text);
                UpdateWeatherDisplay(0);

                // Активируем кнопку "Сегодня"
                TodayBtn.IsChecked = true;
                TomorrowBtn.IsChecked = false;
                DayAfterBtn.IsChecked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CityBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            // Сбрасываем другие кнопки
            var buttons = new[] { TodayBtn, TomorrowBtn, DayAfterBtn };
            foreach (var btn in buttons)
            {
                if (btn != button)
                {
                    btn.IsChecked = false;
                }
            }

            // Обновляем отображение для выбранного дня
            int dayIndex = button == TodayBtn ? 0 : button == TomorrowBtn ? 1 : 2;
            UpdateWeatherDisplay(dayIndex);
        }

        private void CityBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CityBox.Text == "Введите город...")
            {
                CityBox.Text = "";
                CityBox.Foreground = Brushes.Black;
            }
        }

        private void CityBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CityBox.Text))
            {
                CityBox.Text = "Введите город...";
                CityBox.Foreground = Brushes.Gray;
            }
        }
    }
}