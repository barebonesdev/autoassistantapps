using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantLibrary.Items;
using SQLite;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemUnderVehicle : BaseDataItem
    {
        private const string VEHICLEIDENTIFIER = "VehicleIdentifier";

        /// <summary>
        /// Vehicle identifier cannot be changed
        /// </summary>
        [Column(VEHICLEIDENTIFIER)]
        public Guid VehicleIdentifier { get; set; }

        protected override void PopulateSyncItemCoreProperties(BaseSyncItem syncItem)
        {
            (syncItem as BaseSyncItemUnderVehicle).VehicleIdentifier = VehicleIdentifier;

            base.PopulateSyncItemCoreProperties(syncItem);
        }

        protected override void DeserializeCoreProperties(BaseSyncItem item)
        {
            var modItem = item as BaseSyncItemUnderVehicle;

            if (modItem.VehicleIdentifier != null)
                VehicleIdentifier = modItem.VehicleIdentifier.Value;

            base.DeserializeCoreProperties(item);
        }
    }
}
