using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AutoAssistantUWP.Converters
{
    public class SyncStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.MainScreenViewModel.SyncStates)
            {
                switch ((AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.MainScreenViewModel.SyncStates)value)
                {
                    case AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.MainScreenViewModel.SyncStates.Done:
                        return "Sync";

                    case AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.MainScreenViewModel.SyncStates.Syncing:
                        return "Syncing...";

                    case AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.MainScreenViewModel.SyncStates.UploadingImages:
                        return "Uploading images...";

                    default:
                        throw new NotImplementedException("Unknown sync state");
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
