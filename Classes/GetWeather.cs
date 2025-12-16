using Newtonsoft.Json;
using System.Net.Http;
using Weather.Models;


namespace Weather.Classes
{
    public class GetWeather
    {
        public static string Url = "https://api.weather.yandex.ru/v2/forecast";
        public static string Key = "demo.yandex_weather.api.key_code0893M9bao";

        public static async Task<string> Get(float lat, float lon)
        {
            string requestUrl = $"{Url}?lat={lat}&lon={lon}".Replace(",", ".");

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    requestUrl))
                {
                    request.Headers.Add("X-Yandex-Weather-Key", Key);

                    using (var response = await client.SendAsync(request))
                    {
                        string dataResponse = await response.Content.ReadAsStringAsync();
                        return dataResponse;
                    }
                }
            }
        }
    }
}