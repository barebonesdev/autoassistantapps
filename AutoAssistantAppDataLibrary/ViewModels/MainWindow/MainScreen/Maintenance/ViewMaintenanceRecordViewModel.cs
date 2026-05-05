using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Extensions;
using System.Runtime.CompilerServices;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class ViewMaintenanceRecordViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemMaintenanceRecordEntry RecordEntry { get; private set; }

        public string DoneByString
        {
            get
            {
                return GetBindedValue(nameof(RecordEntry.DoneBy), delegate
                {
                    if (string.IsNullOrWhiteSpace(RecordEntry.DoneBy))
                    {
                        return null;
                    }

                    return "Done by: " + RecordEntry.DoneBy.Trim();
                });
            }
        }

        public ViewMaintenanceRecordViewModel(MainScreenViewModel parent, ViewItemMaintenanceRecordEntry recordEntry) : base(parent)
        {
            RecordEntry = recordEntry;

            this.ListenToItem(recordEntry.Identifier).Deleted += ViewMaintenanceRecordViewModel_Deleted;
        }

        private string GetBindedValue(string recordEntryPropertyName, Func<string> convert, [CallerMemberName]string propertyName = null)
        {
            return GetBindedValue(RecordEntry, recordEntryPropertyName, convert, propertyName);
        }

        private void ViewMaintenanceRecordViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            MainScreenViewModel.ShowPopup(AddMaintenanceRecordViewModel.CreateForEdit(MainScreenViewModel, RecordEntry));
        }

        public async void Delete()
        {
            try
            {
                await MainScreenViewModel.DeleteMaintenanceRecord(RecordEntry.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewScheduleItem(ViewItemMaintenanceScheduleItem item)
        {
            MainScreenViewModel.ShowPopup(new ViewScheduleItemViewModel(MainScreenViewModel, item));
        }
    }
}
