using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantLibrary.Items;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.DataLayer;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewItemsGroup;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class AddFuelViewModel : BaseMainScreenViewModelChild
    {
        private static DateTime _prevDate;
        private static DateTime _prevDateSetOn;

        public ViewItemVehicle Vehicle { get; private set; }
        public VehicleViewItemsGroup Fuel { get; private set; }

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        private AddFuelViewModel(MainScreenViewModel parent) : base(parent)
        {
            Vehicle = parent.CurrentVehicle;
            if (Vehicle == null)
            {
                throw new NullReferenceException("CurrentVehicle was null");
            }
        }

        public ViewItemFuelEntry FuelToEdit { get; private set; }

        private bool _showTotalCost;
        public bool ShowTotalCost
        {
            get { return _showTotalCost; }
            set { SetProperty(ref _showTotalCost, value, nameof(ShowTotalCost)); }
        }

        private bool _showMileage;
        public bool ShowMileage
        {
            get { return _showMileage; }
            set { SetProperty(ref _showMileage, value, nameof(ShowMileage)); }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); }
        }

        private decimal _costPerGallon = -1;
        public decimal CostPerGallon
        {
            get { return _costPerGallon; }
            set { SetCostPerGallon(value); UpdateTotalCost(); }
        }

        private void SetCostPerGallon(decimal value)
        {
            SetProperty(ref _costPerGallon, Math.Round(value, 3), nameof(CostPerGallon));
        }

        private decimal _totalCost = -1;
        public decimal TotalCost
        {
            get { return _totalCost; }
            set { SetTotalCost(value); UpdateCostPerGallon(); }
        }

        private void SetTotalCost(decimal value)
        {
            SetProperty(ref _totalCost, Math.Round(value, 2), nameof(TotalCost));
        }

        private decimal _gallons = -1;
        public decimal Gallons
        {
            get { return _gallons; }
            set
            {
                SetProperty(ref _gallons, value, nameof(Gallons));

                if (ShowTotalCost)
                {
                    UpdateCostPerGallon();
                }
                else
                {
                    UpdateTotalCost();
                }

                UpdateMpg();
            }
        }

        private string _storeName = "";
        public string StoreName
        {
            get { return _storeName; }
            set { SetProperty(ref _storeName, value, nameof(StoreName)); }
        }

        private string _location = "";
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value, nameof(Location)); }
        }

        private string _notes = "";
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value, nameof(Notes)); }
        }

        public SyncItemFuelEntry.FuelTypes[] AvailableFuelTypes { get; } = new SyncItemFuelEntry.FuelTypes[]
        {
            SyncItemFuelEntry.FuelTypes.Oct87,
            SyncItemFuelEntry.FuelTypes.Oct89,
            SyncItemFuelEntry.FuelTypes.Oct91,
            SyncItemFuelEntry.FuelTypes.Diesel
        };

        private SyncItemFuelEntry.FuelTypes _fuelType = SyncItemFuelEntry.FuelTypes.Oct87;
        public SyncItemFuelEntry.FuelTypes FuelType
        {
            get { return _fuelType; }
            set { SetProperty(ref _fuelType, value, nameof(FuelType)); }
        }

        private decimal _mileage = -1;
        public decimal Mileage
        {
            get { return _mileage; }
            set { SetMileage(value); UpdateMilesSinceLast(); UpdateMpg(); }
        }

        private void SetMileage(decimal value)
        {
            SetProperty(ref _mileage, value, nameof(Mileage));
        }

        private decimal _milesSinceLast = -1;
        public decimal MilesSinceLast
        {
            get { return _milesSinceLast; }
            set { SetMilesSinceLast(value); UpdateMileage(); UpdateMpg(); }
        }

        private void SetMilesSinceLast(decimal value)
        {
            SetProperty(ref _milesSinceLast, value, nameof(MilesSinceLast));
        }

        private decimal _mpg;
        public decimal MPG
        {
            get { return _mpg; }
            private set { SetProperty(ref _mpg, value, nameof(MPG)); }
        }

        private int _prevFuelIndex = -1;

        private void UpdateMilesSinceLast()
        {
            _prevFuelIndex = -1;
            bool set = false;
            //find where the odometer goes in the list
            for (int i = 0; i < Fuel.Fuel.Count; i++)   // make sure we're not using the one we're editing
                if (Fuel.Fuel[i].Mileage <= Mileage && (FuelToEdit == null || Fuel.Fuel[i].Identifier != FuelToEdit.Identifier))
                {
                    //assign the miles driven
                    SetMilesSinceLast((Mileage - Fuel.Fuel[i].Mileage));

                    _prevFuelIndex = i;
                    set = true;
                    break;
                }

            if (!set)
            {
                SetMilesSinceLast(-1);
            }
        }

        private void UpdateMileage()
        {
            _prevFuelIndex = -1;
            bool set = false;
            //set the odometer higher than the last fillup
            for (int i = 0; i < Fuel.Fuel.Count; i++)
            {
                //make sure we're not using the one we're editing
                if (FuelToEdit == null || Fuel.Fuel[i].Identifier != FuelToEdit.Identifier)
                {
                    SetMileage((Fuel.Fuel[i].Mileage + MilesSinceLast));

                    _prevFuelIndex = i;
                    set = true;
                    break;
                }
            }

            if (!set)
            {
                SetMileage(-1);
            }
        }

        private bool _partialFill = false;
        public bool PartialFill
        {
            get { return _partialFill; }
            set { SetProperty(ref _partialFill, value, nameof(PartialFill)); UpdateMpg(); }
        }

        private bool _skippedEnteringPreviousFillup = false;
        public bool SkippedEnteringPreviousFillup
        {
            get { return _skippedEnteringPreviousFillup; }
            set { SetProperty(ref _skippedEnteringPreviousFillup, value, nameof(SkippedEnteringPreviousFillup)); UpdateMpg(); }
        }

        private void UpdateCostPerGallon()
        {
            if (TotalCost == -1)
            {
                SetCostPerGallon(-1);
                return;
            }

            if (Gallons == -1 || Gallons == 0)
            {
                SetCostPerGallon(-1);
                return;
            }

            SetCostPerGallon(TotalCost / Gallons);
        }

        private void UpdateTotalCost()
        {
            if (CostPerGallon == -1 || Gallons == -1)
            {
                SetTotalCost(-1);
                return;
            }

            SetTotalCost(CostPerGallon * Gallons);
        }

        private void UpdateMpg()
        {
            if (PartialFill || SkippedEnteringPreviousFillup || MilesSinceLast == -1 || Gallons == -1 || _prevFuelIndex == -1)
            {
                MPG = Constants.NO_MPG;
                return;
            }

            int notUsed;
            MPG = Fuel.CalculateMpg(Mileage, Gallons, _prevFuelIndex, out notUsed);
        }

        protected override async Task LoadAsyncOverride()
        {
            Fuel = await VehicleViewItemsGroup.LoadAsync(Vehicle);

            if (State == OperationState.Editing)
            {
                UpdateMilesSinceLast();
                UpdateMpg();
                UpdateTotalCost();
            }
            else
            {
                if (Fuel.Fuel.Count > 0)
                {
                    FuelType = Fuel.Fuel.First().FuelType;
                }
            }

            await base.LoadAsyncOverride();
        }

        public static AddFuelViewModel CreateForAdd(MainScreenViewModel parent)
        {
            return new AddFuelViewModel(parent)
            {
                // TODO:AA - Remember previous selected FuelType
            }.InitializeForAdd();
        }

        public static AddFuelViewModel CreateForEdit(MainScreenViewModel parent, ViewItemFuelEntry itemToEdit)
        {
            return new AddFuelViewModel(parent)
            {
                FuelToEdit = itemToEdit,
                Date = itemToEdit.Date,
                _costPerGallon = itemToEdit.CostPerGallon,
                _gallons = itemToEdit.Gallons,
                StoreName = itemToEdit.StoreName,
                Location = itemToEdit.Location,
                FuelType = itemToEdit.FuelType,
                _mileage = itemToEdit.Mileage,
                PartialFill = itemToEdit.PartialFill,
                SkippedEnteringPreviousFillup = itemToEdit.SkippedEnteringPreviousFillup,
                Notes = itemToEdit.Notes,
                State = OperationState.Editing
            }.InitializeForEdit();
        }

        private AddFuelViewModel InitializeForAdd()
        {
            Initialize();

            FuelType = Vehicle.FuellAddingOption_FuelType;

            if (_prevDateSetOn.AddMinutes(1) > DateTime.Now)
            {
                Date = _prevDate;
            }

            return this;
        }

        private AddFuelViewModel InitializeForEdit()
        {
            Initialize();

            return this;
        }

        private AddFuelViewModel Initialize()
        {
            ShowTotalCost = Vehicle.FuelAddingOption_ShowTotalCost;
            ShowMileage = Vehicle.FuelAddingOption_ShowMileage;

            return this;

        }

        public async void Save()
        {
            try
            {
                if (ShowMileage)
                {
                    if (Mileage == -1)
                    {
                        await new PortableMessageDialog("You must enter your odometer", "No odometer").ShowAsync();
                        return;
                    }
                }
                else
                {
                    // TODO: Miles since
                }

                if (ShowTotalCost)
                {
                    if (TotalCost == -1)
                    {
                        await new PortableMessageDialog("You must enter the total cost", "No total cost").ShowAsync();
                        return;
                    }

                    if (TotalCost < 0)
                    {
                        await new PortableMessageDialog("Total cost must be a positive number.", "Invalid total cost").ShowAsync();
                        return;
                    }
                }
                else
                {
                    if (CostPerGallon == -1)
                    {
                        await new PortableMessageDialog("You must enter the cost per gallon", "No cost per gallon").ShowAsync();
                        return;
                    }

                    if (CostPerGallon < 0)
                    {
                        await new PortableMessageDialog("Cost per gallon must be a positive number.", "Invalid cost per gallon").ShowAsync();
                        return;
                    }
                }

                if (Gallons <= 0)
                {
                    await new PortableMessageDialog("Gallons must be a positive number.", "Invalid gallons").ShowAsync();
                    return;
                }

                DataItemFuelEntry fuel;

                if (FuelToEdit != null)
                    fuel = new DataItemFuelEntry()
                    {
                        Identifier = FuelToEdit.Identifier
                    };

                else
                    fuel = new DataItemFuelEntry()
                    {
                        Identifier = Guid.NewGuid(),
                        VehicleIdentifier = Vehicle.Identifier
                    };

                fuel.CostPerGallon = CostPerGallon;
                fuel.Date = DateTime.SpecifyKind(Date.Date, DateTimeKind.Utc);
                fuel.FuelType = FuelType;
                fuel.Gallons = Gallons;
                fuel.Location = Location.Trim();
                fuel.Mileage = Mileage;
                fuel.PartialFill = PartialFill;
                fuel.SkippedEnteringPreviousFillup = SkippedEnteringPreviousFillup;
                fuel.StoreName = StoreName.Trim();
                fuel.Notes = Notes.Trim();

                _prevDate = Date.Date;
                _prevDateSetOn = DateTime.Now;

                DataChanges changes = new DataChanges();
                changes.Fuel.Add(fuel);

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
