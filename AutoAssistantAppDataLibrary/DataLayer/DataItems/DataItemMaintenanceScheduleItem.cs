using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantLibrary.Items;
using SQLite;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems
{
    public class DataItemMaintenanceScheduleItem : BaseDataItemUnderVehicle
    {
        public static readonly DataItemProperty TitleProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceScheduleItem.Title));

        [Column("Title")]
        public string Title
        {
            get { return GetValue(TitleProperty, ""); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DataItemProperty DetailsProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceScheduleItem.Details));

        [Column("Details")]
        public string Details
        {
            get { return GetValue(DetailsProperty, ""); }
            set { SetValue(DetailsProperty, value); }
        }

        public static readonly DataItemProperty MileageIntervalProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceScheduleItem.MileageInterval));

        [Column("MileageInterval")]
        public decimal MileageInterval
        {
            get { return GetValue<decimal>(MileageIntervalProperty, -1); }
            set { SetValue(MileageIntervalProperty, value); }
        }

        public static readonly DataItemProperty MonthIntervalProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceScheduleItem.MonthInterval));

        [Column("MonthInterval")]
        public short MonthInterval
        {
            get { return GetValue<short>(MonthIntervalProperty, -1); }
            set { SetValue(MonthIntervalProperty, value); }
        }

        public static readonly DataItemProperty EstimatedCostProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceScheduleItem.EstimatedCost));

        [Column("EstimatedCost")]
        public decimal EstimatedCost
        {
            get { return GetValue<decimal>(EstimatedCostProperty, -1); }
            set { SetValue(EstimatedCostProperty, value); }
        }

        protected override Type GetSyncItemType()
        {
            return typeof(SyncItemMaintenanceScheduleItem);
        }
    }
}
