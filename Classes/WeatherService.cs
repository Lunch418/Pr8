using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weather.Models;

namespace Weather.Classes
{
    public static class WeatherService
    {
        private const int DailyLimit = 3;

        public static async Task<DataResponse> GetWeatherCached(string city)
        {
            using (var db = new WeatherDbContext())
            {
                var cache = db.Cache.FirstOrDefault(x => x.City == city);

                if (cache != null && (DateTime.Now - cache.SavedAt).TotalHours < 1)
                {
                    return JsonConvert.DeserializeObject<DataResponse>(cache.JsonData);
                }
                var today = DateTime.Today;

                var usage = db.ApiUsage.FirstOrDefault(x => x.Day == today);
                if (usage == null)
                {
                    usage = new ApiUsage { Day = today, Count = 0 };
                    db.ApiUsage.Add(usage);
                }

                if (usage.Count >= DailyLimit)
                    throw new Exception("Лимит запросов на сегодня исчерпан");
                var (lat, lon) = await GeoCoder.GetCoords(city);
                var data = await GetWeather.Get(lat, lon);

                usage.Count++;
                db.SaveChanges();
                if (cache == null)
                {
                    cache = new WeatherCache
                    {
                        City = city,
                        Lat = lat,
                        Lon = lon,
                        JsonData = JsonConvert.SerializeObject(data),
                        SavedAt = DateTime.Now
                    };
                    db.Cache.Add(cache);
                }
                else
                {
                    cache.JsonData = JsonConvert.SerializeObject(data);
                    cache.SavedAt = DateTime.Now;
                }

                db.SaveChanges();

                return data;
            }
        }
    }
}
