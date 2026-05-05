using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class MaintenanceViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemVehicle Vehicle { get; private set; }

        private VehicleViewItemsGroup _viewItemsGroup;

        public MyObservableList<ViewItemMaintenanceScheduleItem> ScheduleItems { get; private set; }
        public MyObservableList<ViewItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; }

        private bool _hasOverdueServices;
        public bool HasOverdueServices
        {
            get { return _hasOverdueServices; }
            private set { SetProperty(ref _hasOverdueServices, value, nameof(HasOverdueServices)); }
        }

        private bool _hasNextServices;
        public bool HasNextServices
        {
            get { return _hasNextServices; }
            private set { SetProperty(ref _hasNextServices, value, nameof(HasNextServices)); }
        }

        private bool _hasFutureServices;
        public bool HasFutureServices
        {
            get { return _hasFutureServices; }
            private set { SetProperty(ref _hasFutureServices, value, nameof(HasFutureServices)); }
        }

        private bool _hasNoServices;
        public bool HasNoServices
        {
            get { return _hasNoServices; }
            private set { SetProperty(ref _hasNoServices, value, nameof(HasNoServices)); }
        }

        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> OverdueServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();
        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> NextServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();
        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> FutureServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();

        public MaintenanceViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        protected override async Task LoadAsyncOverride()
        {
            _viewItemsGroup = await VehicleViewItemsGroup.LoadAsync(Vehicle);
            _viewItemsGroup.OnUpcomingServicesReset += new WeakEventHandler<EventArgs>(_viewItemsGroup_OnUpcomingServicesReset).Handler;

            ScheduleItems = _viewItemsGroup.MaintenanceSchedule;
            OnPropertyChanged(nameof(ScheduleItems));

            MaintenanceRecords = _viewItemsGroup.MaintenanceRecords;
            OnPropertyChanged(nameof(MaintenanceRecords));

            ResetUpcomingSchedule();
        }

        private void _viewItemsGroup_OnUpcomingServicesReset(object sender, EventArgs e)
        {
            ResetUpcomingSchedule();
        }

        public void AddScheduleItem()
        {
            MainScreenViewModel.ShowPopup(AddScheduleItemViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewScheduleItem(ViewItemMaintenanceScheduleItem item)
        {
            MainScreenViewModel.ShowPopup(new ViewScheduleItemViewModel(MainScreenViewModel, item));
        }

        public void AddMaintenanceRecord()
        {
            MainScreenViewModel.ShowPopup(AddMaintenanceRecordViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewMaintenanceRecord(ViewItemMaintenanceRecordEntry entry)
        {
            MainScreenViewModel.ShowPopup(new ViewMaintenanceRecordViewModel(MainScreenViewModel, entry));
        }

        public void ViewUpcomingService(ViewItemUpcomingMaintenanceScheduleItem item)
        {
            ViewScheduleItem(item.ScheduleItem);
        }

        private void ResetUpcomingSchedule()
        {
            DateTime today = DateTime.Today;
            decimal estimatedMileage = Vehicle.EstimatedMileage;

            var desiredOverdueServices = _viewItemsGroup.UpcomingServices.Where(i => (i.DateNeededAt != Constants.NO_DATE && i.DateNeededAt.Date < today) || (i.MilesNeededAt != Constants.NO_MILES && i.MilesNeededAt < estimatedMileage)).ToList();
            MakeUpcomingServicesListLike(OverdueServices, desiredOverdueServices);

            var nextServices = new List<ViewItemUpcomingMaintenanceScheduleItem>();
            DateTime firstDate = Constants.NO_DATE;
            decimal firstMileage = Constants.NO_MILES;
            foreach (var service in _viewItemsGroup.UpcomingServices.Except(desiredOverdueServices))
            {
                if (nextServices.Count == 0)
                {
                    nextServices.Add(service);
                    if (service.IsDateSooner)
                    {
                        if (service.DateNeededAt == Constants.NO_DATE)
                        {
                            firstDate = Vehicle.EstimateDateOn(service.MilesNeededAt);
                        }
                        else
                        {
                            firstDate = service.DateNeededAt;
                        }

                        // Give it wiggle room of 7 days in future
                        firstDate = firstDate.AddDays(7);
                        firstMileage = Vehicle.EstimatedMileageOn(firstDate);
                    }
                    else
                    {
                        if (service.MilesNeededAt == Constants.NO_MILES)
                        {
                            firstMileage = Vehicle.EstimatedMileageOn(firstDate);
                        }
                        else
                        {
                            firstMileage = service.MilesNeededAt;
                        }

                        // Give it wiggle room of 7 days in future
                        firstMileage = firstMileage + Vehicle.EstimatedMilesPerDay * 7;
                        firstDate = Vehicle.EstimateDateOn(firstMileage);
                    }
                }
                else
                {
                    if (service.DateNeededAt != Constants.NO_DATE && service.DateNeededAt <= firstDate
                        || service.MilesNeededAt != Constants.NO_MILES && service.MilesNeededAt <= firstMileage)
                    {
                        nextServices.Add(service);
                    }
                }
            }
            MakeUpcomingServicesListLike(NextServices, nextServices);

            MakeUpcomingServicesListLike(FutureServices, _viewItemsGroup.UpcomingServices.Except(desiredOverdueServices).Except(nextServices).ToList());

            HasOverdueServices = OverdueServices.Count > 0;
            HasNextServices = NextServices.Count > 0;
            HasFutureServices = FutureServices.Count > 0;
            HasNoServices = !HasOverdueServices && !HasNextServices && !HasFutureServices;
        }

        private void MakeUpcomingServicesListLike(MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> mainList, IList<ViewItemUpcomingMaintenanceScheduleItem> finalList)
        {
            List<ViewItemUpcomingMaintenanceScheduleItem> intermediateList = new List<ViewItemUpcomingMaintenanceScheduleItem>(finalList.Count);

            for (int i = 0; i < finalList.Count; i++)
            {
                var finalItem = finalList[i];

                if (i == mainList.Count)
                {
                    intermediateList.Add(finalItem);
                }
                else
                {
                    bool found = false;

                    for (int x = i; x < mainList.Count; x++)
                    {
                        // If found matching schedule item
                        if (mainList[x].ScheduleItem == finalItem.ScheduleItem)
                        {
                            // Update it
                            mainList[x].Initialize(finalItem);

                            // Add it in correct location
                            intermediateList.Add(mainList[x]);

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        intermediateList.Add(finalItem);
                    }
                }
            }

            mainList.MakeListLike(intermediateList);
        }

        public void SearchRecords()
        {
            MainScreenViewModel.ShowPopup(new SearchMaintenanceRecordsViewModel(MainScreenViewModel, MainScreenViewModel.CurrentVehicle));
        }
    }
}
