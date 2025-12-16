using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Weather.Classes
{
    public class CityValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string city = value as string;

            if (string.IsNullOrWhiteSpace(city))
            {
                return new ValidationResult(false, "Введите название города");
            }
            return ValidationResult.ValidResult;
        }
    }
}