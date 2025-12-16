using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Weather.Classes;
using Weather.Elements;
using Weather.Models;

namespace Weather
{
    public partial class MainWindow : Window
    {
    
        private List<string> popularCities = new List<string>
{
    "Москва", "Санкт-Петербург", "Екатеринбург", "Новосибирск",
    "Казань", "Нижний Новгород", "Челябинск", "Самара", "Омск",
    "Ростов-на-Дону", "Уфа", "Красноярск", "Пермь", "Воронеж",
    "Волгоград", "Краснодар", "Саратов", "Тюмень", "Тольятти"
};
        DataResponce? responce;
        private float currentLat = 58.009671f;
        private float currentLon = 56.226184f;
        private string currentCity = "Пермь";
        private readonly CacheService _cacheService;
        private readonly DispatcherTimer _cleanupTimer;

        public MainWindow()
        {
            InitializeComponent();
            _cacheService = new CacheService();
            LocationText.Text = currentCity;

            _cleanupTimer = new DispatcherTimer();
            _cleanupTimer.Interval = TimeSpan.FromHours(6);
            _cleanupTimer.Tick += CleanupTimer_Tick;
            _cleanupTimer.Start();

            InitializeApplication();
        }

        private async void InitializeApplication()
        {
            try
            {
                await _cacheService.CleanupOldCacheAsync();
                await LoadWeatherData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CleanupTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                await _cacheService.CleanupOldCacheAsync();
            }
            catch
            {
               
            }
        }

        public async Task LoadWeatherData(bool forceApi = false)
        {
            parent.Children.Clear();
            Days.Items.Clear();

            try
            {
                DataResponce? weatherData = null;
                bool fromCache = false;

                if (!forceApi)
                {
                    var cachedData = await _cacheService.GetCachedWeatherAsync(currentCity, currentLat, currentLon);
                    if (cachedData != null)
                    {
                        weatherData = cachedData;
                        fromCache = true;
                        responce = weatherData;
                    }
                }

                if (weatherData == null || forceApi)
                {
                    try
                    {
                        weatherData = await GetWeather.Get(currentLat, currentLon, currentCity, forceApi);
                        responce = weatherData;
                        fromCache = false;
                    }
                    catch (Exception apiEx)
                    {
                        if (!forceApi)
                        {
                            weatherData = await _cacheService.GetCachedWeatherAsync(currentCity, currentLat, currentLon);
                            if (weatherData != null)
                            {
                                fromCache = true;
                                responce = weatherData;

                                Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show($"Используются кэшированные данные. API недоступно: {apiEx.Message}",
                                        "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
                                });
                            }
                            else
                            {
                                throw new Exception($"API недоступно: {apiEx.Message}", apiEx);
                            }
                        }
                        else
                        {
                            throw new Exception($"API недоступно: {apiEx.Message}", apiEx);
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    if (weatherData?.forecasts != null && weatherData.forecasts.Count > 0)
                    {
                     
                        Days.Items.Clear();

                        int daysToShow = Math.Min(4, weatherData.forecasts.Count);
                        for (int i = 0; i < daysToShow; i++)
                        {
                            Days.Items.Add(weatherData.forecasts[i].date.ToString("dd.MM.yyyy"));
                        }

                        if (Days.Items.Count > 0)
                        {
                            Days.SelectedIndex = 0;
                        }

                        string sourceInfo = fromCache ? " (из кэша)" : " (актуальные)";
                        LocationText.Text = $"{currentCity}{sourceInfo}";

                        UpdateCurrentWeatherUI(weatherData);
                    }
                    else
                    {
                        MessageBox.Show("Нет данных о прогнозе погоды",
                            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка загрузки погоды: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public void Create(int idForecast)
        {
            if (responce?.forecasts == null ||
                idForecast < 0 ||
                idForecast >= responce.forecasts.Count)
            {
                return;
            }

            var forecast = responce.forecasts[idForecast];

            if (forecast.hours == null || forecast.hours.Count == 0)
            {
                return;
            }

            parent.Children.Clear();

            foreach (Hour hour in forecast.hours)
            {
                parent.Children.Add(new Elements.Item(hour));
            }
        }

        private void UpdateCurrentWeatherUI(DataResponce? weatherData)
        {
            if (weatherData?.forecasts == null || weatherData.forecasts.Count == 0)
                return;

            var forecast = weatherData.forecasts[0];
            if (forecast.hours == null || forecast.hours.Count == 0)
                return;

            var currentHour = DateTime.Now.Hour;

           
            var currentDayHour = forecast.hours
                .OrderBy(h => Math.Abs(int.Parse(h.hour) - currentHour))
                .FirstOrDefault()
                ?? forecast.hours.First();

            Dispatcher.Invoke(() =>
            {
                if (currentDayHour != null)
                {
                    lTempCurrent.Text = $"{currentDayHour.temp}°";
                    lConditionCurrent.Text = currentDayHour.ToCondition();

                    
                    // lWindCurrent.Text = $"{currentDayHour.wind_speed} м/с";
                    // lHumidityCurrent.Text = $"{currentDayHour.humidity}%";
                    // lPressureCurrent.Text = $"{currentDayHour.pressure_mm} мм";
                }
            });
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string cityName = CityTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(cityName))
            {
                MessageBox.Show("Введите название города", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var originalContent = SearchButton.Content;
            var originalIsEnabled = SearchButton.IsEnabled;

            try
            {
                SearchButton.IsEnabled = false;
                SearchButton.Content = "Поиск...";

                
                var coordinates = await Geocoding.GetCoordinates(cityName);
                currentLat = coordinates.lat;
                currentLon = coordinates.lon;
                currentCity = cityName;

             
                var cachedData = await _cacheService.GetCachedWeatherAsync(currentCity, currentLat, currentLon);
                if (cachedData != null)
                {
                   
                    responce = cachedData;
                    UpdateUIFromCachedData(cachedData, true);
                    return;
                }

              
                bool canRequest = await _cacheService.CanMakeRequestAsync();
                if (!canRequest)
                {
               
                    var remaining = await _cacheService.GetRemainingRequestsAsync();
                    var nextTime = await _cacheService.GetNextRequestTimeAsync();

                    if (nextTime.HasValue)
                    {
                        var timeLeft = nextTime.Value - DateTime.Now;
                        MessageBox.Show($"Лимит запросов исчерпан. Попробуйте позже через {timeLeft.Minutes} минут.\nМожно попробовать другой город, данные которого уже есть в кэше.",
                            "Лимит запросов", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    
                    ShowCachedCities();
                    return;
                }

              
                await LoadWeatherData(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска города: {ex.Message}\nПроверьте название и попробуйте снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SearchButton.IsEnabled = originalIsEnabled;
                SearchButton.Content = originalContent;
            }
        }

        private void UpdateUIFromCachedData(DataResponce cachedData, bool fromCache)
        {
            Dispatcher.Invoke(() =>
            {
                if (cachedData?.forecasts != null && cachedData.forecasts.Count > 0)
                {
             
                    Days.Items.Clear();

                    int daysToShow = Math.Min(4, cachedData.forecasts.Count);
                    for (int i = 0; i < daysToShow; i++)
                    {
                        Days.Items.Add(cachedData.forecasts[i].date.ToString("dd.MM.yyyy"));
                    }

              
                    if (Days.Items.Count > 0)
                    {
                        Days.SelectedIndex = 0;
                    }

                    string sourceInfo = fromCache ? " (из кэша)" : " (актуальные)";
                    LocationText.Text = $"{currentCity}{sourceInfo}";

                 
                    responce = cachedData;
                    Create(0);
                }
            });
        }

        private async void ShowCachedCities()
        {
           
            using (var db = new WeatherDbContext())
            {
                var cachedCities = await db.WeatherCaches
                    .AsNoTracking()
                    .Select(c => c.City)
                    .Distinct()
                    .ToListAsync();

                if (cachedCities.Any())
                {
                    string citiesList = string.Join("\n", cachedCities);
                    MessageBox.Show($"Достигнут лимит запросов. Доступны данные из кэша для городов:\n{citiesList}\n\nВведите название одного из этих городов.",
                        "Выберите город из кэша", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Кэш пуст. Дождитесь возможности сделать запрос к API.",
                        "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SelectDay(object sender, SelectionChangedEventArgs e)
        {
            if (Days.SelectedIndex >= 0 &&
                responce?.forecasts != null &&
                Days.SelectedIndex < responce.forecasts.Count)
            {
                Create(Days.SelectedIndex);
            }
        }

        private async void UpdateWeather(object sender, RoutedEventArgs e)
        {
            var originalContent = (sender as Button)?.Content;
            var originalIsEnabled = (sender as Button)?.IsEnabled ?? true;

            try
            {
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Content = "Обновление...";
                }

                await LoadWeatherData(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sender is Button button)
                {
                    button.IsEnabled = originalIsEnabled;
                    button.Content = originalContent;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _cleanupTimer.Stop();
            _cacheService?.Dispose();
            base.OnClosed(e);
        }
       

    }
}