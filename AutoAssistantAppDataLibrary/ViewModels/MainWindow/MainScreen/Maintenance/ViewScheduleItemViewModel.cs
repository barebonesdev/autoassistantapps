using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class ViewScheduleItemViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemMaintenanceScheduleItem ScheduleItem { get; private set; }

        public MyObservableList<ViewItemMaintenanceRecordEntry> Records { get; private set; }

        private VehicleViewItemsGroup _vehicleViewItemsGroup;

        public ViewScheduleItemViewModel(MainScreenViewModel parent, ViewItemMaintenanceScheduleItem scheduleItem) : base(parent)
        {
            ScheduleItem = scheduleItem;

            this.ListenToItem(scheduleItem.Identifier).Deleted += ViewScheduleItemViewModel_Deleted;

            LoadRecords();
        }

        private async void LoadRecords()
        {
            try
            {
                _vehicleViewItemsGroup = await VehicleViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentVehicle);
                Records = _vehicleViewItemsGroup.MaintenanceRecords.Sublist(record => record.ServicesPerformed.Any(service => service.Identifier == ScheduleItem.Identifier));
                OnPropertyChanged(nameof(Records));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewScheduleItemViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            MainScreenViewModel.ShowPopup(AddScheduleItemViewModel.CreateForEdit(MainScreenViewModel, ScheduleItem));
        }

        public async void Delete()
        {
            try
            {
                await MainScreenViewModel.DeleteMaintenanceSchedule(ScheduleItem.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewRecord(ViewItemMaintenanceRecordEntry record)
        {
            MainScreenViewModel.ShowPopup(new ViewMaintenanceRecordViewModel(MainScreenViewModel, record));
        }
    }
}
