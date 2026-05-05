using AutoAssistantUWP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using static AutoAssistantAppDataLibrary.DataLayer.NavigationManager;

namespace AutoAssistantUWP.Converters
{
    public class MainMenuSelectionToMainMenuItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MainMenuSelections)
            {
                switch ((MainMenuSelections)value)
                {
                    case MainMenuSelections.Overview:
                        return new MainScreenView.MainMenuItem()
                        {
                            Glyph = "\uE7EC",
                            Title = "Overview"
                        };

                    case MainMenuSelections.Fuel:
                        return new MainScreenView.MainMenuItem()
                        {
                            Glyph = "\uEC4A", // or \uE16E
                            Title = "Fuel"
                        };

                    case MainMenuSelections.Maintenance:
                        return new MainScreenView.MainMenuItem()
                        {
                            Glyph = "\uE15E",
                            Title = "Maintenance"
                        };

                    case MainMenuSelections.Garage:
                        return new MainScreenView.MainMenuItem()
                        {
                            Glyph = "\uE10F",
                            Title = "Garage"
                        };
                } // Settings is \uE115
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}