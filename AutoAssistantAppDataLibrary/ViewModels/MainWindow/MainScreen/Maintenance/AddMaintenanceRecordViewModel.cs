using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class AddMaintenanceRecordViewModel : BaseMainScreenViewModelChild
    {
        public AccountDataItem Account { get; private set; }
        public ViewItemVehicle Vehicle { get; private set; }

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public ViewItemMaintenanceRecordEntry ItemToEdit { get; private set; }

        private string _title = "";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value, nameof(Title)); }
        }

        private decimal _mileage = Constants.NO_MILES;
        public decimal Mileage
        {
            get { return _mileage; }
            set { SetProperty(ref _mileage, value, nameof(Mileage)); }
        }

        public string MileageString
        {
            get => Mileage == Constants.NO_MILES ? "" : Mileage.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Mileage = Constants.NO_MILES;
                }
                else if (decimal.TryParse(value, out decimal result))
                {
                    Mileage = result;
                }
                else
                {
                    // Invalid
                    Mileage = 0;
                }
            }
        }

        private decimal _cost = Constants.NO_COST;
        public decimal Cost
        {
            get { return _cost; }
            set { SetProperty(ref _cost, value, nameof(Cost)); }
        }

        public MyObservableList<ViewItemMaintenanceScheduleItem> ServicesPerformed { get; private set; }

        public MyObservableList<ViewItemMaintenanceScheduleItem> AllServices { get; private set; }

        public MyObservableList<ViewItemMaintenanceScheduleItem> UnselectedServices { get; private set; }

        public string[] AllDoneByEntries { get; private set; }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); }
        }

        private string _doneBy = "";
        public string DoneBy
        {
            get { return _doneBy; }
            set { SetProperty(ref _doneBy, value, nameof(DoneBy)); }
        }

        private string _details = "";
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        private AddMaintenanceRecordViewModel(MainScreenViewModel parent) : base(parent)
        {
            Vehicle = parent.CurrentVehicle;
            if (Vehicle == null)
            {
                throw new NullReferenceException("CurrentVehicle was null");
            }
        }

        public static AddMaintenanceRecordViewModel CreateForAdd(MainScreenViewModel parent)
        {
            return new AddMaintenanceRecordViewModel(parent)
            {
                State = OperationState.Adding
            };
        }

        public static AddMaintenanceRecordViewModel CreateForEdit(MainScreenViewModel parent, ViewItemMaintenanceRecordEntry item)
        {
            return new AddMaintenanceRecordViewModel(parent)
            {
                State = OperationState.Editing,
                ItemToEdit = item,
                Title = item.Title,
                Mileage = item.Mileage,
                Cost = item.Cost,
                AllServices = item.AllScheduleItems,
                ServicesPerformed = item.ServicesPerformed,
                Date = item.Date,
                DoneBy = item.DoneBy,
                Details = item.Details
            };
        }

        private VehicleViewItemsGroup _vehicleGroup;
        protected override async Task LoadAsyncOverride()
        {
            _vehicleGroup = await VehicleViewItemsGroup.LoadAsync(Vehicle);

            if (AllServices == null)
            {
                AllServices = _vehicleGroup.MaintenanceSchedule;
                ServicesPerformed = new MyObservableList<ViewItemMaintenanceScheduleItem>();
                OnPropertyChanged(nameof(AllServices), nameof(ServicesPerformed));
            }

            UnselectedServices = AllServices.Sublist(i => !ServicesPerformed.Contains(i));
            OnPropertyChanged(nameof(UnselectedServices));

            AllDoneByEntries = _vehicleGroup.MaintenanceRecords.Select(i => i.DoneBy).Where(i => !string.IsNullOrWhiteSpace(i)).Distinct(new StringComparer()).OrderBy(i => i).ToArray();

            await base.LoadAsyncOverride();
        }

        private class StringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }

        public void SelectService(ViewItemMaintenanceScheduleItem item)
        {
            UnselectedServices.Remove(item);

            if (!ServicesPerformed.Contains(item))
            {
                ServicesPerformed.Add(item);
            }
        }

        public void UnselectService(ViewItemMaintenanceScheduleItem item)
        {
            ServicesPerformed.Remove(item);

            if (!UnselectedServices.Contains(item))
            {
                UnselectedServices.Add(item);
            }
        }

        public async void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    await new PortableMessageDialog("You must enter a title.", "No title").ShowAsync();
                    return;
                }

                if (Mileage != Constants.NO_MILES && Mileage <= 0)
                {
                    await new PortableMessageDialog("Mileage must be a positive number greater than 0, or left blank if unknown.", "Invalid mileage").ShowAsync();
                    return;
                }

                DataItemMaintenanceRecordEntry dataItem;

                if (ItemToEdit != null)
                    dataItem = new DataItemMaintenanceRecordEntry()
                    {
                        Identifier = ItemToEdit.Identifier
                    };

                else
                    dataItem = new DataItemMaintenanceRecordEntry()
                    {
                        Identifier = Guid.NewGuid(),
                        VehicleIdentifier = Vehicle.Identifier
                    };

                dataItem.Title = Title.Trim();
                dataItem.Mileage = Mileage;
                dataItem.Cost = Cost;
                dataItem.ServicesPerformed = ServicesPerformed.Select(i => i.Identifier).ToArray();
                dataItem.Date = DateTime.SpecifyKind(Date.Date, DateTimeKind.Utc);
                dataItem.DoneBy = DoneBy;
                dataItem.Details = Details.Trim();

                DataChanges changes = new DataChanges();
                changes.MaintenanceRecords.Add(dataItem);

                await AutoAssistantApp.Current.SaveChanges(changes);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Failed to save. Your error has been reported.", "Error saving").ShowAsync();
                return;
            }

            base.GoBack();
        }
    }
}
