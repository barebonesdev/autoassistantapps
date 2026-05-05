using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class DecimalToTextBoxTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal && (decimal)value == -1)
            {
                return "";
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                decimal d;
                if (decimal.TryParse(value as string, out d))
                {
                    return d;
                }
            }

            return -1m;
        }
    }
}
