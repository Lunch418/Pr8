using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Weather.Models;

namespace Weather.Elements
{
    public partial class Items : UserControl
    {
        public Items(string timeOfDay, List<Hour> hours)
        {
            InitializeComponent();

            if (hours == null || hours.Count == 0)
                return;

            var minTemp = hours.Min(h => h.temp);
            var maxTemp = hours.Max(h => h.temp);
            var avgHumidity = (int)hours.Average(h => h.humidity);
            var avgPressure = (int)hours.Average(h => h.pressure);
            var avgWindSpeed = Math.Round(hours.Average(h => h.wind_speed), 1);
            var avgFeelsLike = (int)hours.Average(h => h.feels_like);
            var mostCommonCondition = hours.GroupBy(h => h.condition)
                                          .OrderByDescending(g => g.Count())
                                          .First()
                                          .First();

            lTimeOfDay.Text = timeOfDay;
            lTempRange.Text = $"+{minTemp}°...+{maxTemp}°";
            lCondition.Text = mostCommonCondition.ToCondition();
            lHumidity.Text = $"{avgHumidity}%";
            lPressure.Text = $"{avgPressure}";
            lWind.Text = $"{avgWindSpeed} {mostCommonCondition.GetWindDirection()}";
            lFeelsLike.Text = $"+{avgFeelsLike}°";
        }
    }
}