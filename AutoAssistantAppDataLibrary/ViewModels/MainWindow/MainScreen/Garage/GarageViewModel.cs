using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage
{
    public class GarageViewModel : BaseMainScreenViewModelChild
    {
        private GarageViewItemsGroup _garageViewItemsGroup;
        public MyObservableList<ViewItemVehicle> Vehicles { get; private set; }

        public GarageViewModel(MainScreenViewModel parent) : base(parent)
        {
        }

        protected override async Task LoadAsyncOverride()
        {
            _garageViewItemsGroup = await GarageViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId);
            Vehicles = _garageViewItemsGroup.Vehicles;
            OnPropertyChanged(nameof(Vehicles));
        }

        public void AddVehicle()
        {
            MainScreenViewModel.ShowPopup(AddVehicleViewModel.CreateForAdd(MainScreenViewModel));
        }

        public Task OpenVehicle(ViewItemVehicle vehicle)
        {
            return MainScreenViewModel.SetCurrentVehicleAsync(vehicle.Identifier);
        }
    }
}
