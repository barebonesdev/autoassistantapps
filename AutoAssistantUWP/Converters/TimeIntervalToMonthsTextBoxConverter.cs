using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class TimeIntervalToMonthsTextBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                return ((TimeSpan)value).TotalDays / 30d;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                double months;
                if (double.TryParse(value as string, out months))
                {
                    return TimeSpan.FromDays(months * 30);
                }
            }

            return value;
        }
    }
}
