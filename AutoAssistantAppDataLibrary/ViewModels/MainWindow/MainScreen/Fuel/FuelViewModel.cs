using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Helpers;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class FuelViewModel : BaseMainScreenViewModelChild
    {
        public VehicleViewItemsGroup _fuelViewItemsGroup;
        public bool IsFuelLoaded { get; private set; }
        public MyObservableList<ViewItemFuelEntry> FuelEntries { get; private set; }
        public ViewItemVehicle Vehicle { get; private set; }

        private decimal _mpgAtLastRefill = Constants.NO_MPG;
        private string _mpgAtLastRefillString = "--";
        public string MpgAtLastRefillString
        {
            get { return _mpgAtLastRefillString; }
            private set { SetProperty(ref _mpgAtLastRefillString, value, nameof(MpgAtLastRefillString)); }
        }

        private decimal _mpgInLast1000Miles = Constants.NO_MPG;
        private string _mpgInLast1000MilesString = "--";
        public string MpgInLast1000MilesString
        {
            get { return _mpgInLast1000MilesString; }
            private set { SetProperty(ref _mpgInLast1000MilesString, value, nameof(MpgInLast1000MilesString)); }
        }

        private decimal _mpgInLast3000Miles = Constants.NO_MPG;
        private string _mpgInLast3000MilesString = "--";
        public string MpgInLast3000MilesString
        {
            get { return _mpgInLast3000MilesString; }
            private set { SetProperty(ref _mpgInLast3000MilesString, value, nameof(MpgInLast3000MilesString)); }
        }

        private decimal _overallMpg = Constants.NO_MPG;
        private string _overallMpgString = "--";
        public string OverallMpgString
        {
            get { return _overallMpgString; }
            private set { SetProperty(ref _overallMpgString, value, nameof(OverallMpgString)); }
        }

        private bool _manuallySetEstimatorMpg;
        private decimal _estimatorMpg = Constants.NO_MPG;
        public decimal EstimatorMpg
        {
            get { return _estimatorMpg; }
            set { SetProperty(ref _estimatorMpg, value, nameof(EstimatorMpg)); _manuallySetEstimatorMpg = true; UpdateEstimatedCost(); }
        }

        private bool _manuallySetEstimatorCostPerGallon;
        private decimal _estimatorCostPerGallon = Constants.NO_COST;
        public decimal EstimatorCostPerGallon
        {
            get { return _estimatorCostPerGallon; }
            set { SetProperty(ref _estimatorCostPerGallon, value, nameof(EstimatorCostPerGallon)); _manuallySetEstimatorCostPerGallon = true; UpdateEstimatedCost(); }
        }

        private decimal _estimatorDistance = 100;
        public decimal EstimatorDistance
        {
            get { return _estimatorDistance; }
            set { SetProperty(ref _estimatorDistance, value, nameof(EstimatorDistance)); UpdateEstimatedCost(); }
        }

        private decimal _estimatorTotalCost = Constants.NO_COST;
        public decimal EstimatorTotalCost
        {
            get { return _estimatorTotalCost; }
            private set { SetProperty(ref _estimatorTotalCost, value, nameof(EstimatorTotalCost)); }
        }

        private decimal _estimatorTotalGallons = Constants.NO_GALLONS;
        public decimal EstimatorTotalGallons
        {
            get { return _estimatorTotalGallons; }
            private set { SetProperty(ref _estimatorTotalGallons, value, nameof(EstimatorTotalGallons)); }
        }

        private string _estimatorTotalCostString = "--";
        public string EstimatorTotalCostString
        {
            get { return _estimatorTotalCostString; }
            private set { SetProperty(ref _estimatorTotalCostString, value, nameof(EstimatorTotalCostString)); }
        }

        private string _estimatorTotalGallonsString = "--";
        public string EstimatorTotalGallonsString
        {
            get { return _estimatorTotalGallonsString; }
            private set { SetProperty(ref _estimatorTotalGallonsString, value, nameof(EstimatorTotalGallonsString)); }
        }

        public FuelViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        protected override async Task LoadAsyncOverride()
        {
            await LoadFuelEntriesAsync();
        }

        private void AccountDataStore_DataChangedEvent(object sender, DataChangedEvent e)
        {
            try
            {
                // If there were fuel changes
                if (e.LocalAccountId == MainScreenViewModel.CurrentLocalAccountId && e.Fuel.HasChanges())
                {
                    PortableDispatcher.GetCurrentDispatcher().Run(UpdateFuelStats);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task LoadFuelEntriesAsync()
        {
            _fuelViewItemsGroup = await VehicleViewItemsGroup.LoadAsync(Vehicle);

            FuelEntries = _fuelViewItemsGroup.Fuel;

            // Watch for changes
            _fuelViewItemsGroup.OnChangesMade += new WeakEventHandler<EventArgs>(_fuelViewItemsGroup_OnChangesMade).Handler;

            UpdateFuelStats();

            IsFuelLoaded = true;
            OnPropertyChanged(nameof(IsFuelLoaded), nameof(FuelEntries));
        }

        private void _fuelViewItemsGroup_OnChangesMade(object sender, EventArgs e)
        {
            UpdateFuelStats();
        }

        private void UpdateFuelStats()
        {
            try
            {
                _overallMpg = _fuelViewItemsGroup.CalculageMpgInLastMiles(decimal.MaxValue);
                OverallMpgString = AutoAssistantStringFormatter.FormatMpg(_overallMpg);

                _mpgAtLastRefill = Constants.NO_MPG;
                if (FuelEntries.Count > 0)
                {
                    _mpgAtLastRefill = FuelEntries[0].MPG;
                }
                MpgAtLastRefillString = AutoAssistantStringFormatter.FormatMpg(_mpgAtLastRefill);

                _mpgInLast1000Miles = _fuelViewItemsGroup.CalculageMpgInLastMiles(1000);
                MpgInLast1000MilesString = AutoAssistantStringFormatter.FormatMpg(_mpgInLast1000Miles);

                _mpgInLast3000Miles = _fuelViewItemsGroup.CalculageMpgInLastMiles(3000);
                MpgInLast3000MilesString = AutoAssistantStringFormatter.FormatMpg(_mpgInLast3000Miles);

                if (!_manuallySetEstimatorMpg)
                {
                    if (_mpgInLast1000Miles == Constants.NO_MPG)
                    {
                        EstimatorMpg = 25;
                    }
                    else
                    {
                        EstimatorMpg = Math.Round(_mpgInLast1000Miles, 1);
                    }
                }
                if (!_manuallySetEstimatorCostPerGallon)
                {
                    decimal[] costPerGallons = _fuelViewItemsGroup.Fuel.Where(i => i.CostPerGallon != Constants.NO_COST).Take(5).Select(i => i.CostPerGallon).ToArray();
                    if (costPerGallons.Length == 0)
                    {
                        EstimatorCostPerGallon = 3.10m;
                    }
                    else
                    {
                        EstimatorCostPerGallon = Math.Round(costPerGallons.Average(), 2);
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void AddFuel()
        {
            MainScreenViewModel.ShowPopup(AddFuelViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewFuelEntry(ViewItemFuelEntry entry)
        {
            MainScreenViewModel.ShowPopup(new ViewFuelViewModel(MainScreenViewModel, entry));
        }

        private void UpdateEstimatedCost()
        {
            if (EstimatorMpg == Constants.NO_MPG || EstimatorCostPerGallon == Constants.NO_COST || EstimatorDistance == Constants.NO_MILES
                || EstimatorMpg == 0)
            {
                EstimatorTotalGallons = Constants.NO_GALLONS;
                EstimatorTotalCost = Constants.NO_COST;
            }

            else
            {
                EstimatorTotalGallons = EstimatorDistance / EstimatorMpg;
                EstimatorTotalCost = EstimatorTotalGallons * EstimatorCostPerGallon;
            }

            EstimatorTotalGallonsString = AutoAssistantStringFormatter.FormatGallons(EstimatorTotalGallons);
            EstimatorTotalCostString = AutoAssistantStringFormatter.FormatCost(EstimatorTotalCost);
        }

        public void ImportFuel()
        {
            MainScreenViewModel.ShowPopup(new ImportFuelIntroViewModel(MainScreenViewModel));
        }

        public void ExportFuel()
        {
            MainScreenViewModel.ShowPopup(new ExportFuelToCsvViewModel(MainScreenViewModel));
        }
    }
}