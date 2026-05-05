using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantLibrary.Items;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemFuelEntry : BaseViewItemUnderVehicle, IComparable<ViewItemFuelEntry>
    {
        public ViewItemFuelEntry(Guid identifier) : base(identifier) { }
        public ViewItemFuelEntry(DataItemFuelEntry dataItem) : base(dataItem) { }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); }
        }

        private decimal _costPerGallon;
        public decimal CostPerGallon
        {
            get { return _costPerGallon; }
            set { SetProperty(ref _costPerGallon, value, nameof(CostPerGallon)); }
        }

        private decimal _gallons;
        public decimal Gallons
        {
            get { return _gallons; }
            set { SetProperty(ref _gallons, value, nameof(Gallons)); }
        }

        private decimal _totalCost;
        public decimal TotalCost
        {
            get { return _totalCost; }
            set { SetProperty(ref _totalCost, value, nameof(TotalCost)); }
        }

        private string _storeName;
        public string StoreName
        {
            get { return _storeName; }
            set { SetProperty(ref _storeName, value, nameof(StoreName)); }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value, nameof(Location)); }
        }

        private SyncItemFuelEntry.FuelTypes _fuelType;
        public SyncItemFuelEntry.FuelTypes FuelType
        {
            get { return _fuelType; }
            set { SetProperty(ref _fuelType, value, nameof(FuelType)); }
        }

        private decimal _mileage;
        public decimal Mileage
        {
            get { return _mileage; }
            set { SetProperty(ref _mileage, value, nameof(Mileage)); }
        }

        private bool _partialFill;
        public bool PartialFill
        {
            get { return _partialFill; }
            set { SetProperty(ref _partialFill, value, nameof(PartialFill)); }
        }

        private bool _skippedEnteringPreviousFillup;
        public bool SkippedEnteringPreviousFillup
        {
            get { return _skippedEnteringPreviousFillup; }
            set { SetProperty(ref _skippedEnteringPreviousFillup, value, nameof(SkippedEnteringPreviousFillup)); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value, nameof(Notes)); }
        }

        private decimal _mpg = -1;
        public decimal MPG
        {
            get { return _mpg; }
            internal set { SetProperty(ref _mpg, value, nameof(MPG)); }
        }

        public decimal _milesSinceLast = -1;
        public decimal MilesSinceLast
        {
            get { return _milesSinceLast; }
            internal set { SetProperty(ref _milesSinceLast, value, nameof(MilesSinceLast)); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            var i = dataItem as DataItemFuelEntry;

            Date = DateTime.SpecifyKind(i.Date, DateTimeKind.Local);
            CostPerGallon = i.CostPerGallon;
            Gallons = i.Gallons;
            StoreName = i.StoreName;
            Location = i.Location;
            FuelType = i.FuelType;
            Mileage = i.Mileage;
            PartialFill = i.PartialFill;
            SkippedEnteringPreviousFillup = i.SkippedEnteringPreviousFillup;
            Notes = i.Notes;

            if (CostPerGallon != Constants.NO_COST && Gallons != Constants.NO_GALLONS)
            {
                TotalCost = CostPerGallon * Gallons;
            }
            else
            {
                TotalCost = Constants.NO_COST;
            }
        }

        protected override IEnumerable<BaseViewItemWithoutData> EnumerateAllChildrenThatHaveChildren()
        {
            return new BaseViewItemWithoutData[0];
        }

        internal override bool HandleModifyingChildren(DataChangedEvent e)
        {
            return false;
        }

        internal override bool HandleUpdatingSelfAndDescendants(DataChangedEvent e)
        {
            // Edit this item itself
            var editedItem = e.Fuel.EditedItems.FirstOrDefault(i => i.Identifier == Identifier);
            if (editedItem != null)
            {
                PopulateFromDataItem(editedItem);
                return true;
            }

            return false;
        }

        public int CompareTo(ViewItemFuelEntry other)
        {
            int comp = other.Mileage.CompareTo(Mileage);

            if (comp == 0)
            {
                return base.CompareTo(other);
            }

            return comp;
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemFuelEntry)
            {
                return CompareTo(other as ViewItemFuelEntry);
            }

            return base.CompareTo(other);
        }
    }
}
