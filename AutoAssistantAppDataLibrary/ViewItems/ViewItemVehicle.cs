using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantLibrary.Items;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemVehicle : BaseViewItem
    {
        public ViewItemVehicle(Guid identifier) : base(identifier) { }
        public ViewItemVehicle(DataItemVehicle dataItem) : base(dataItem) { }

        private string _nickname;
        public string Nickname
        {
            get { return _nickname; }
            set { SetProperty(ref _nickname, value, nameof(Nickname)); }
        }

        private string _make;
        public string Make
        {
            get { return _make; }
            set { SetProperty(ref _make, value, nameof(Make)); }
        }

        private string _model;
        public string Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value, nameof(Model)); }
        }

        private string _year;
        public string Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value, nameof(Year)); }
        }

        private string _yearMakeModelString;
        public string YearMakeModelString
        {
            get { return _yearMakeModelString; }
            set { SetProperty(ref _yearMakeModelString, value, nameof(YearMakeModelString)); }
        }

        private string _licensePlate;
        public string LicensePlate
        {
            get { return _licensePlate; }
            set { SetProperty(ref _licensePlate, value, nameof(LicensePlate)); }
        }

        private string _vin;
        public string VIN
        {
            get { return _vin; }
            set { SetProperty(ref _vin, value, nameof(VIN)); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value, nameof(Notes)); }
        }

        private DateTime _datePurchased;
        public DateTime DatePurchased
        {
            get { return _datePurchased; }
            set { SetProperty(ref _datePurchased, value, nameof(DatePurchased)); }
        }

        private decimal _initialMileage;
        public decimal InitialMileage
        {
            get { return _initialMileage; }
            set { SetProperty(ref _initialMileage, value, nameof(InitialMileage)); }
        }

        private string _purchasedFrom;
        public string PurchasedFrom
        {
            get { return _purchasedFrom; }
            set { SetProperty(ref _purchasedFrom, value, nameof(PurchasedFrom)); }
        }

        private string _amountPurchasedFor;
        public string AmountPurchasedFor
        {
            get { return _amountPurchasedFor; }
            set { SetProperty(ref _amountPurchasedFor, value, nameof(AmountPurchasedFor)); }
        }

        /// <summary>
        /// Gets the current estimated mileage, based on the latest gas fillup, and the estimated miles per day.
        /// </summary>
        public decimal EstimatedMileage
        {
            get
            {
                return EstimatedMileageOn(DateTime.Today);
            }
        }

        private decimal _lastKnownMileage;
        public decimal LastKnownMileage
        {
            get { return _lastKnownMileage; }
            set { SetProperty(ref _lastKnownMileage, value, nameof(LastKnownMileage)); }
        }

        private DateTime _lastKnownMileageRecordedOn;
        public DateTime LastKnownMileageRecordedOn
        {
            get { return _lastKnownMileageRecordedOn; }
            set { SetProperty(ref _lastKnownMileageRecordedOn, value, nameof(LastKnownMileageRecordedOn)); }
        }

        private decimal _estimatedMilesPerDay;
        public decimal EstimatedMilesPerDay
        {
            get { return _estimatedMilesPerDay; }
            set { SetProperty(ref _estimatedMilesPerDay, value, nameof(EstimatedMilesPerDay)); }
        }

        /// <summary>
        /// Whether the total cost or the cost per gallon should be displayed by default when adding a fuel
        /// </summary>
        public bool FuelAddingOption_ShowTotalCost { get; set; }

        /// <summary>
        /// Whether the vehicle's total mileage or the miles since last should be displayed by default when adding a fuel
        /// </summary>
        public bool FuelAddingOption_ShowMileage { get; set; }

        public SyncItemFuelEntry.FuelTypes FuellAddingOption_FuelType { get; set; }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemVehicle i = (DataItemVehicle)dataItem;

            Nickname = i.Nickname;
            Make = i.Make;
            Model = i.Model;
            Year = i.Year;
            LicensePlate = i.LicensePlate;
            VIN = i.VIN;
            Notes = i.Notes;
            DatePurchased = DateTime.SpecifyKind(i.DatePurchased, DateTimeKind.Local);
            InitialMileage = i.InitialMileage;
            PurchasedFrom = i.PurchasedFrom;
            AmountPurchasedFor = i.AmountPurchasedFor;
            FuelAddingOption_ShowMileage = i.FuelAddingOption_ShowMileage;
            FuelAddingOption_ShowTotalCost = i.FuelAddingOption_ShowTotalCost;
            FuellAddingOption_FuelType = i.FuelAddingOption_FuelType;

            YearMakeModelString = string.Join(" ", new string[] { Year, Make, Model }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        /// <summary>
        /// Estimates the miles that would be driven over the given timespan
        /// </summary>
        /// <returns></returns>
        public decimal EstimateMilesDriven(TimeSpan span)
        {
            //33 miles per day * 5 days = 165 miles driven over that timespan
            return EstimatedMilesPerDay * Math.Abs(span.Days);
        }

        /// <summary>
        /// Returns the estimated mileage on the given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal EstimatedMileageOn(DateTime date)
        {
            if (LastKnownMileageRecordedOn == Constants.NO_DATE)
                return LastKnownMileage;

            return LastKnownMileage + EstimateMilesDriven(date - LastKnownMileageRecordedOn);
        }

        /// <summary>
        /// Estimates what the date was most likely on that mileage, based on LastKnownMileage and estimated miles driven per day
        /// </summary>
        /// <param name="mileage"></param>
        /// <returns></returns>
        public DateTime EstimateDateOn(decimal mileage)
        {
            if (LastKnownMileageRecordedOn == Constants.NO_DATE)
                return DateTime.Today;


            decimal mileageDifference = mileage - LastKnownMileage;

            int days = (int)(mileageDifference / EstimatedMilesPerDay);

            return LastKnownMileageRecordedOn.AddDays(days);
        }

        public void RecalculateMileageStats(VehicleViewItemsGroup vehicleGroup)
        {
            decimal answer = 0;

            if (InitialMileage != Constants.NO_MILES)
            {
                answer = InitialMileage;
            }

            var mileageHistory = vehicleGroup.Fuel.Select(i => new ViewItemMileageEntry(i)).Union(vehicleGroup.MaintenanceRecords.Where(i => i.Mileage != Constants.NO_MILES).Select(i => new ViewItemMileageEntry(i))).ToList();
            mileageHistory.Sort();

            RecalculateLastKnownMileage(mileageHistory);
            RecalculateEstimatedMilesPerDay(mileageHistory);

            OnPropertyChanged(nameof(EstimatedMileage));
        }

        private void RecalculateLastKnownMileage(List<ViewItemMileageEntry> mileageHistory)
        {
            if (mileageHistory.Count == 0)
            {
                LastKnownMileage = InitialMileage;
                LastKnownMileageRecordedOn = DateTime.MinValue;
            }

            else
            {
                LastKnownMileage = mileageHistory.Last().Mileage;
                LastKnownMileageRecordedOn = mileageHistory.Last().Date;
            }
        }

        private static readonly int DAYS_TO_KEEP_FOR_MILEAGE = 100;
        private void RecalculateEstimatedMilesPerDay(List<ViewItemMileageEntry> mileageHistory)
        {
            if (mileageHistory.Count >= 2)
            {
                decimal miles = 0;
                int days = 0;

                for (int i = mileageHistory.Count - 2; i >= 0; i--)
                {
                    if (days > DAYS_TO_KEEP_FOR_MILEAGE)
                        break;

                    miles += mileageHistory[i + 1].Mileage - mileageHistory[i].Mileage;
                    days += Math.Abs((mileageHistory[i + 1].Date - mileageHistory[i].Date).Days);
                }

                if (days > 0 && miles > 0)
                {
                    EstimatedMilesPerDay = miles / days;
                    return;
                }
            }

            EstimatedMilesPerDay = 33; //33 is from 12,000 miles driven per year
        }

        public DateTime? GetEstimatedDateBorn()
        {
            if (Year != null && int.TryParse(Year, out int year) && year > 1900 && year < 3000)
            {
                return new DateTime(year, 1, 1);
            }

            return null;
        }

        internal override bool HandleUpdatingSelfAndDescendants(DataChangedEvent e)
        {
            bool changed = false;

            // First edit this item itself
            DataItemVehicle editedVehicle = e.Vehicles.EditedItems.FirstOrDefault(i => i.Identifier == Identifier);
            if (editedVehicle != null)
            {
                PopulateFromDataItem(editedVehicle);
                changed = true;
            }

            // Children are now stored in separate view groups so they can dispose
            // Then edit all children
            //foreach (var child in EnumerateAllChildren())
            //{
            //    if (child.HandleUpdatingSelfAndDescendants(e))
            //    {
            //        changed = true;
            //    }
            //}

            return changed;
        }

        internal override bool HandleModifyingChildren(DataChangedEvent e)
        {
            return false;
        }

        //private bool HandleModifyingChildren<V, D>(MyObservableList<V> list, DataChangedEvent.ScopedDataChangedEvent<D> e)
        //    where V : BaseViewItemUnderVehicle
        //    where D : BaseDataItemUnderVehicle, new()
        //{
        //    bool changed = false;

        //    List<V> toRemove = new List<V>();
        //    List<V> toReSort = new List<V>();

        //    foreach (V child in list)
        //    {
        //        // If it was deleted, then we mark it for remove
        //        if (e.DeletedItems.Contains(child.Identifier))
        //            toRemove.Add(child);

        //        else
        //        {
        //            D edited = e.EditedItems.OfType<D>().FirstOrDefault(i => i.Identifier == child.Identifier);

        //            // If it was edited
        //            if (edited != null)
        //            {
        //                // We'll need to re-sort it
        //                toReSort.Add(child);
        //            }
        //        }
        //    }

        //    if (toRemove.Count > 0 || toReSort.Count > 0)
        //        changed = true;

        //    // Now remove all that need removing
        //    foreach (V item in toRemove)
        //        list.Remove(item);

        //    // And re-sort all that need re-sorting
        //    foreach (V item in toReSort)
        //    {
        //        // First remove
        //        list.Remove(item);

        //        // Then re-add
        //        list.InsertSorted(item);
        //    }

        //    // And now add the new items
        //    if (AddChildren(list, e.NewItems.Where(i => i.VehicleIdentifier == Identifier)))
        //    {
        //        changed = true;
        //    }

        //    return changed;
        //}

        //internal void InitializeFuel(IEnumerable<DataItemFuelEntry> dataFuel)
        //{
        //    Fuel = new MyObservableList<ViewItemFuelEntry>();
        //    AddChildren(Fuel, dataFuel);
        //}

        //internal void InitializeMaintenanceRecords(IEnumerable<DataItemMaintenanceRecordEntry> dataMaintenanceRecords)
        //{
        //    MaintenanceRecords = new MyObservableList<ViewItemMaintenanceRecordEntry>();
        //    AddChildren(MaintenanceRecords, dataMaintenanceRecords);
        //}

        //internal void InitializeMaintenanceSchedule(IEnumerable<DataItemMaintenanceScheduleItem> dataMaintenanceSchedule)
        //{
        //    MaintenanceSchedule = new MyObservableList<ViewItemMaintenanceScheduleItem>();
        //    AddChildren(MaintenanceSchedule, dataMaintenanceSchedule);
        //}

        //private bool AddChildren<V, D>(MyObservableList<V> list, IEnumerable<D> dataItems)
        //    where V : BaseViewItemUnderVehicle
        //    where D : BaseDataItemUnderVehicle, new()
        //{
        //    int before = list.Count;
        //    list.InsertSorted(dataItems.Select(i => CreateChild<V>(i)), trackChanges: false);
        //    return list.Count > before;
        //}

        //private V CreateChild<V>(BaseDataItemUnderVehicle dataItem)
        //    where V : BaseViewItemUnderVehicle
        //{
        //    var viewType = typeof(V);
        //    BaseViewItemUnderVehicle answer;

        //    if (viewType == typeof(ViewItemFuelEntry))
        //    {
        //        answer = new ViewItemFuelEntry((DataItemFuelEntry)dataItem);
        //    }
        //    else if (viewType == typeof(ViewItemMaintenanceRecordEntry))
        //    {
        //        answer = new ViewItemMaintenanceRecordEntry((DataItemMaintenanceRecordEntry)dataItem);
        //    }
        //    else if (viewType == typeof(ViewItemMaintenanceScheduleItem))
        //    {
        //        answer = new ViewItemMaintenanceScheduleItem((DataItemMaintenanceScheduleItem)dataItem);
        //    }
        //    else
        //    {
        //        throw new NotImplementedException("Unknown type " + viewType);
        //    }

        //    answer.Vehicle = this;
        //    return answer as V;
        //}

        //private IEnumerable<BaseViewItemWithoutData> EnumerateAllChildren()
        //{
        //    if (Fuel != null)
        //    {
        //        foreach (var f in Fuel)
        //        {
        //            yield return f;
        //        }
        //    }

        //    if (MaintenanceRecords != null)
        //    {
        //        foreach (var r in MaintenanceRecords)
        //        {
        //            yield return r;
        //        }
        //    }

        //    if (MaintenanceSchedule != null)
        //    {
        //        foreach (var s in MaintenanceSchedule)
        //        {
        //            yield return s;
        //        }
        //    }
        //}

        protected override IEnumerable<BaseViewItemWithoutData> EnumerateAllChildrenThatHaveChildren()
        {
            // None of the children have children
            return new BaseViewItemWithoutData[0];
        }

        //private Task _loadFuelTask;
        //public Task LoadFuelAsync()
        //{
        //    lock (this)
        //    {
        //        if (_loadFuelTask == null)
        //        {
        //            _loadFuelTask = PerformDataStoreOperation(LoadFuelOperation, nameof(Fuel));
        //        }
        //    }

        //    return _loadFuelTask;
        //}

        //private void LoadFuelOperation(AccountDataStore dataStore)
        //{
        //    DataItemFuelEntry[] fuel = dataStore.TableFuel.Where(i => i.VehicleIdentifier == Identifier).ToArray();

        //    InitializeFuel(fuel);
        //}

        //private Task _loadMaintenanceTask;
        //public Task LoadMaintenanceAsync()
        //{
        //    lock (this)
        //    {
        //        if (_loadMaintenanceTask == null)
        //        {
        //            _loadMaintenanceTask = PerformDataStoreOperation(LoadMaintenanceOperation, nameof(MaintenanceRecords), nameof(MaintenanceSchedule));
        //        }
        //    }

        //    return _loadMaintenanceTask;
        //}

        //private void LoadMaintenanceOperation(AccountDataStore dataStore)
        //{
        //    DataItemMaintenanceRecordEntry[] records = dataStore.TableMaintenanceRecords.Where(i => i.VehicleIdentifier == Identifier).ToArray();
        //    DataItemMaintenanceScheduleItem[] schedules = dataStore.TableMaintenanceSchedules.Where(i => i.VehicleIdentifier == Identifier).ToArray();

        //    InitializeMaintenanceRecords(records);
        //    InitializeMaintenanceSchedule(schedules);
        //}

        //private AccountDataStore _dataStore;
        //private async Task PerformDataStoreOperation(Action<AccountDataStore> action, params string[] notifyProps)
        //{
        //    await Task.Run(async delegate
        //    {
        //        if (_dataStore == null)
        //        {
        //            _dataStore = await AccountDataStore.Get(Account.LocalAccountId);
        //        }

        //        using (await Locks.LockDataForReadAsync("VehiclesViewItemsGroup.LoadBlocking"))
        //        {
        //            action(_dataStore);
        //        }
        //    });

        //    // On the UI thread we notify that the list property was set
        //    OnPropertyChanged(notifyProps);
        //}

        private MaintenanceViewModel _maintenanceViewModel;
        public MaintenanceViewModel GetMaintenanceViewModel(MainScreenViewModel mainScreenViewModel)
        {
            if (_maintenanceViewModel == null)
            {
                _maintenanceViewModel = new MaintenanceViewModel(mainScreenViewModel, this);
            }

            return _maintenanceViewModel;
        }

        private bool _startedInitializeUpcomingMaintenance;
        public async void StartInitializeUpcomingMaintenance(MainScreenViewModel mainScreenViewModel)
        {
            try
            {
                if (_startedInitializeUpcomingMaintenance)
                {
                    return;
                }

                _startedInitializeUpcomingMaintenance = true;

                var maintenanceViewModel = GetMaintenanceViewModel(mainScreenViewModel);
                await maintenanceViewModel.LoadAsync();

                if (maintenanceViewModel.HasOverdueServices)
                {
                    MaintenanceStatusType = MaintenanceStatus.Overdue;
                    MaintenanceStatusText = maintenanceViewModel.OverdueServices.Count == 1 ? "1 overdue service" : maintenanceViewModel.OverdueServices.Count + " overdue services";
                }
                else if (maintenanceViewModel.HasNextServices)
                {
                    MaintenanceStatusType = MaintenanceStatus.Upcoming;
                    int count = maintenanceViewModel.NextServices.Count;
                    string text;
                    if (count == 1)
                    {
                        text = "1 service in ";
                    }
                    else
                    {
                        text = count + " services in ";
                    }
                    MaintenanceStatusText = text + maintenanceViewModel.NextServices[0].Counter + maintenanceViewModel.NextServices[0].CounterType;
                }
                else
                {
                    MaintenanceStatusType = MaintenanceStatus.None;
                    MaintenanceStatusText = "No services";
                }
            }
            catch
            {

            }
        }

        public enum MaintenanceStatus
        {
            Loading,
            Overdue,
            Upcoming,
            None
        }

        private MaintenanceStatus _maintenanceStatusType = MaintenanceStatus.Loading;
        public MaintenanceStatus MaintenanceStatusType
        {
            get => _maintenanceStatusType;
            set => SetProperty(ref _maintenanceStatusType, value, nameof(MaintenanceStatusType));
        }

        private string _maintenanceStatusText = "Loading...";
        public string MaintenanceStatusText
        {
            get => _maintenanceStatusText;
            set => SetProperty(ref _maintenanceStatusText, value, nameof(MaintenanceStatusText));
        }
    }
}
