using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantLibrary.Items;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems
{
    public class DataItemFuelEntry : BaseDataItemUnderVehicle
    {
        public static readonly DataItemProperty DateProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.Date));

        [Column("Date")]
        public DateTime Date
        {
            get { return GetValue(DateProperty, SqlDate.MinValue); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DataItemProperty CostPerGallonProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.CostPerGallon));

        [Column("CostPerGallon")]
        public decimal CostPerGallon
        {
            get { return GetValue<decimal>(CostPerGallonProperty, 1); }
            set { SetValue(CostPerGallonProperty, value); }
        }

        public static readonly DataItemProperty GallonsProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.Gallons));

        [Column("Gallons")]
        public decimal Gallons
        {
            get { return GetValue<decimal>(GallonsProperty, 1); }
            set { SetValue(GallonsProperty, value); }
        }

        public static readonly DataItemProperty StoreNameProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.StoreName));

        [Column("StoreName")]
        public string StoreName
        {
            get { return GetValue(StoreNameProperty, ""); }
            set { SetValue(StoreNameProperty, value); }
        }

        public static readonly DataItemProperty LocationProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.Location));

        [Column("Location")]
        public string Location
        {
            get { return GetValue(LocationProperty, ""); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DataItemProperty FuelTypeProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.FuelType));

        [Column("FuelType")]
        public SyncItemFuelEntry.FuelTypes FuelType
        {
            get { return GetValue(FuelTypeProperty, SyncItemFuelEntry.FuelTypes.Oct87); }
            set { SetValue(FuelTypeProperty, value); }
        }

        public static readonly DataItemProperty MileageProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.Mileage));

        [Column("Mileage")]
        public decimal Mileage
        {
            get { return GetValue<decimal>(MileageProperty, 1); }
            set { SetValue(MileageProperty, value); }
        }

        public static readonly DataItemProperty PartialFillProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.PartialFill));

        [Column("PartialFill")]
        public bool PartialFill
        {
            get { return GetValue(PartialFillProperty, false); }
            set { SetValue(PartialFillProperty, value); }
        }

        public static readonly DataItemProperty SkippedEnteringPreviousFillupProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.SkippedEnteringPreviousFillup));

        [Column("SkippedEnteringPreviousFillup")]
        public bool SkippedEnteringPreviousFillup
        {
            get { return GetValue(SkippedEnteringPreviousFillupProperty, false); }
            set { SetValue(SkippedEnteringPreviousFillupProperty, value); }
        }

        public static readonly DataItemProperty NotesProperty = DataItemProperty.Register(nameof(SyncItemFuelEntry.Notes));

        [Column("Notes")]
        public string Notes
        {
            get { return GetValue(NotesProperty, ""); }
            set { SetValue(NotesProperty, value); }
        }

        protected override Type GetSyncItemType()
        {
            return typeof(SyncItemFuelEntry);
        }
    }
}
