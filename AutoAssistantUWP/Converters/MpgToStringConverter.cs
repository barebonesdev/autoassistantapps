using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class MpgToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal)
            {
                if (parameter as string == "NoMpg")
                {
                    if ((decimal)value == -1)
                    {
                        return "--";
                    }

                    return ((decimal)value).ToString("0.0");
                }

                if ((decimal)value == -1)
                {
                    return "-- MPG";
                }

                return ((decimal)value).ToString("0.0") + " MPG";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
