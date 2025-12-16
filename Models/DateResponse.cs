using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather.Models
{
    public class DataResponse
    {
        public List<Forecast> forecasts { get; set; } = new List<Forecast>();
    }

    public class Forecast
    {
        public DateTime date { get; set; }
        public List<Hour> hours { get; set; } = new List<Hour>();
    }

    public class Hour
    {
        public string hour { get; set; } = string.Empty;
        public string condition { get; set; } = string.Empty;
        public int humidity { get; set; }
        public int prec_type { get; set; }
        public int temp { get; set; }
        public int pressure { get; set; } // добавим давление
        public string wind_dir { get; set; } = string.Empty; // направление ветра
        public double wind_speed { get; set; } // скорость ветра
        public int feels_like { get; set; } // ощущается как

        public string ToPrecType()
        {
            string result = "";

            switch (this.prec_type)
            {
                case 0:
                    result = "Без осадков";
                    break;
                case 1:
                    result = "Дождь";
                    break;
                case 2:
                    result = "Дождь со снегом";
                    break;
                case 3:
                    result = "Снег";
                    break;
            }
            return result;
        }

        public string ToCondition()
        {
            string result = "";
            switch (this.condition)
            {
                case "clear":
                    result = "Ясно";
                    break;
                case "partly-cloudy":
                    result = "Малооблачно";
                    break;
                case "cloudy":
                    result = "Облачно с прояснениями";
                    break;
                case "overcast":
                    result = "Пасмурно";
                    break;
                case "light-rain":
                    result = "Небольшой дождь";
                    break;
                case "rain":
                    result = "Дождь";
                    break;
                case "heavy-rain":
                    result = "Сильный дождь";
                    break;
                case "showers":
                    result = "Ливень";
                    break;
                case "wet-snow":
                    result = "Дождь со снегом";
                    break;
                case "light-snow":
                    result = "Небольшой снег";
                    break;
                case "snow":
                    result = "Снег";
                    break;
                case "snow-showers":
                    result = "Снегопад";
                    break;
                case "hail":
                    result = "Град";
                    break;
                case "thunderstorm":
                    result = "Гроза";
                    break;
                case "thunderstorm-with-rain":
                    result = "Дождь с грозой";
                    break;
                case "thunderstorm-with-hail":
                    result = "Гроза с градом";
                    break;
                default:
                    result = this.condition;
                    break;
            }
            return result;
        }

        public string GetWindDirection()
        {
            switch (wind_dir)
            {
                case "nw": return "сз";
                case "n": return "с";
                case "ne": return "св";
                case "e": return "в";
                case "se": return "юв";
                case "s": return "ю";
                case "sw": return "юз";
                case "w": return "з";
                default: return wind_dir;
            }
        }
    }
}