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
    /// <summary>
    /// Логика взаимодействия для Items.xaml
    /// </summary>
    public partial class Items : UserControl
    {
        public Items(Hour hour)
        {
            InitializeComponent();

            lHour.Content = hour.hour;
            lCondition.Content = hour.ToCondition();
            lHumidity.Content = hour.humidity + "%";
            lPrecType.Content = hour.ToPrecType();
            lTemp.Content = hour.temp;
        }
    }
}
