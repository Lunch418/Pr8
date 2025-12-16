using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather.Classes
{
    public class WeatherCache
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string JsonData { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }
}