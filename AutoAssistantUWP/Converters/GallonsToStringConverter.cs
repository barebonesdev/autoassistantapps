using AutoAssistantAppDataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class GallonsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal)
            {
                string answer;
                if ((decimal)value == Constants.NO_GALLONS)
                {
                    answer = "--";
                }
                else
                {
                    answer = ((decimal)value).ToString("0.0");
                }

                if (parameter != null)
                {
                    return answer + " gallons";
                }

                return answer;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
