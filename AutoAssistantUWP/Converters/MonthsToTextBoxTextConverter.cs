using AutoAssistantAppDataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class MonthsToTextBoxTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is short)
            {
                if ((short)value == Constants.NO_MONTHS)
                {
                    return "";
                }

                return value.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var str = value as string;
            if (string.IsNullOrWhiteSpace(str))
            {
                return Constants.NO_MONTHS;
            }

            short answer;
            if (short.TryParse(str, out answer))
            {
                return answer;
            }

            return Constants.NO_MONTHS;
        }
    }
}
