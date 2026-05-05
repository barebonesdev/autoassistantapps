using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
using AutoAssistantAppDataLibrary.Extensions;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Overview
{
    public class OverviewViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemVehicle Vehicle { get; private set; }

        public OverviewViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        public void Edit()
        {
            MainScreenViewModel.ShowPopup(AddVehicleViewModel.CreateForEdit(MainScreenViewModel, Vehicle));
        }

        public async void Delete()
        {
            try
            {
                await MainScreenViewModel.DeleteVehicle(Vehicle.Identifier);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
