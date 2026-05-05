using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantLibrary.Items;
using SQLite;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems
{
    public class DataItemVehicle : BaseDataItem
    {
        public static readonly DataItemProperty NicknameProperty = DataItemProperty.Register(nameof(SyncItemVehicle.Nickname));

        [Column("Nickname")]
        public string Nickname
        {
            get { return GetValue(NicknameProperty, ""); }
            set { SetValue(NicknameProperty, value); }
        }

        public static readonly DataItemProperty MakeProperty = DataItemProperty.Register(nameof(SyncItemVehicle.Make));

        [Column("Make")]
        public string Make
        {
            get { return GetValue(MakeProperty, ""); }
            set { SetValue(MakeProperty, value); }
        }

        public static readonly DataItemProperty ModelProperty = DataItemProperty.Register(nameof(SyncItemVehicle.Model));

        [Column("Model")]
        public string Model
        {
            get { return GetValue(ModelProperty, ""); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DataItemProperty YearProperty = DataItemProperty.Register(nameof(SyncItemVehicle.Year));

        [Column("Year")]
        public string Year
        {
            get { return GetValue(YearProperty, ""); }
            set { SetValue(YearProperty, value); }
        }

        public static readonly DataItemProperty LicensePlateProperty = DataItemProperty.Register(nameof(SyncItemVehicle.LicensePlate));

        [Column("LicensePlate")]
        public string LicensePlate
        {
            get { return GetValue(LicensePlateProperty, ""); }
            set { SetValue(LicensePlateProperty, value); }
        }

        public static readonly DataItemProperty VINProperty = DataItemProperty.Register(nameof(SyncItemVehicle.VIN));

        [Column("VIN")]
        public string VIN
        {
            get { return GetValue(VINProperty, ""); }
            set { SetValue(VINProperty, value); }
        }

        public static readonly DataItemProperty NotesProperty = DataItemProperty.Register(nameof(SyncItemVehicle.Notes));

        [Column("Notes")]
        public string Notes
        {
            get { return GetValue(NotesProperty, ""); }
            set { SetValue(NotesProperty, value); }
        }

        public static readonly DataItemProperty DatePurchasedProperty = DataItemProperty.Register(nameof(SyncItemVehicle.DatePurchased));

        [Column("DatePurchased")]
        public DateTime DatePurchased
        {
            get { return GetValue(DatePurchasedProperty, SqlDate.MinValue); }
            set { SetValue(DatePurchasedProperty, value); }
        }

        public static readonly DataItemProperty InitialMileageProperty = DataItemProperty.Register(nameof(SyncItemVehicle.InitialMileage));

        [Column("InitialMileage")]
        public decimal InitialMileage
        {
            get { return GetValue<decimal>(InitialMileageProperty, 0); }
            set { SetValue(InitialMileageProperty, value); }
        }

        public static readonly DataItemProperty PurchasedFromProperty = DataItemProperty.Register(nameof(SyncItemVehicle.PurchasedFrom));

        [Column("PurchasedFrom")]
        public string PurchasedFrom
        {
            get { return GetValue(PurchasedFromProperty, ""); }
            set { SetValue(PurchasedFromProperty, value); }
        }

        public static readonly DataItemProperty AmountPurchasedForProperty = DataItemProperty.Register(nameof(SyncItemVehicle.AmountPurchasedFor));

        [Column("AmountPurchasedFor")]
        public string AmountPurchasedFor
        {
            get { return GetValue(AmountPurchasedForProperty, ""); }
            set { SetValue(AmountPurchasedForProperty, value); }
        }

        /// <summary>
        /// Whether the total cost or the cost per gallon should be displayed by default when adding a fuel
        /// </summary>
        [Column("FuelAddingOption_ShowTotalCost")]
        public bool FuelAddingOption_ShowTotalCost { get; set; } = true;

        /// <summary>
        /// Whether the vehicle's total mileage or the miles since last should be displayed by default when adding a fuel
        /// </summary>
        [Column("FuelAddingOption_ShowMileage")]
        public bool FuelAddingOption_ShowMileage { get; set; } = true;

        /// <summary>
        /// Default fuel type for the vehicle
        /// </summary>
        [Column("FuelAddingOption_FuelType")]
        public SyncItemFuelEntry.FuelTypes FuelAddingOption_FuelType { get; set; } = SyncItemFuelEntry.FuelTypes.Oct87;

        protected override Type GetSyncItemType()
        {
            return typeof(SyncItemVehicle);
        }
    }
}
