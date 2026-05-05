using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using static AutoAssistantAppDataLibrary.ViewItems.ViewItemVehicle;

namespace AutoAssistantUWP.Converters
{
    public class MaintenanceStatusTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MaintenanceStatus status)
            {
                switch (status)
                {
                    case MaintenanceStatus.Loading:
                    case MaintenanceStatus.None:
                        return Application.Current.Resources["AppBarBackground"];

                    case MaintenanceStatus.Overdue:
                        return new SolidColorBrush(Colors.Red);

                    case MaintenanceStatus.Upcoming:
                        return Application.Current.Resources["SystemControlHighlightAccentBrush"];

                    default:
                        return value;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MaintenanceStatusTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MaintenanceStatus status)
            {
                switch (status)
                {
                    case MaintenanceStatus.Loading:
                    case MaintenanceStatus.None:
                        return Application.Current.Resources["ApplicationForegroundThemeBrush"];

                    case MaintenanceStatus.Overdue:
                    case MaintenanceStatus.Upcoming:
                        return new SolidColorBrush(Colors.White);

                    default:
                        return value;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
