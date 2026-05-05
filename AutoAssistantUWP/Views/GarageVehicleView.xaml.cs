using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AutoAssistantUWP.Views
{
    public sealed partial class GarageVehicleView : UserControl
    {
        public GarageVehicleView()
        {
            this.InitializeComponent();

            DataContextChanged += GarageVehicleView_DataContextChanged;
        }

        private void GarageVehicleView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is ViewItemVehicle vehicle)
            {
                try
                {
                    var mainScreenViewModel = AutoAssistantApp.Current.GetCurrentWindow().ViewModel.FinalContent.FindAncestor<MainScreenViewModel>();
                    vehicle.StartInitializeUpcomingMaintenance(mainScreenViewModel);
                }
                catch
                {

                }
            }
        }
    }
}
