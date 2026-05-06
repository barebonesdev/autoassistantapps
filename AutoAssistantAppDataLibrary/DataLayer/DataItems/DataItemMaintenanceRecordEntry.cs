using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantLibrary.Items;
using ToolsPortable;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems
{
    public class DataItemMaintenanceRecordEntry : BaseDataItemUnderVehicle
    {
        public static readonly DataItemProperty TitleProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.Title));

        [Column("Title")]
        public string Title
        {
            get { return GetValue(TitleProperty, ""); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DataItemProperty DoneByProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.DoneBy));

        [Column("DoneBy")]
        public string DoneBy
        {
            get { return GetValue(DoneByProperty, ""); }
            set { SetValue(DoneByProperty, value); }
        }

        public static readonly DataItemProperty MileageProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.Mileage));

        [Column("Mileage")]
        public decimal Mileage
        {
            get { return GetValue<decimal>(MileageProperty, 1); }
            set { SetValue(MileageProperty, value); }
        }

        public static readonly DataItemProperty DateProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.Date));

        [Column("Date")]
        public DateTime Date
        {
            get { return GetValue(DateProperty, SqlDate.MinValue); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DataItemProperty CostProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.Cost));

        [Column("Cost")]
        public decimal Cost
        {
            get { return GetValue<decimal>(CostProperty, 1); }
            set { SetValue(CostProperty, value); }
        }

        public static readonly DataItemProperty DetailsProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.Details));

        [Column("Details")]
        public string Details
        {
            get { return GetValue(DetailsProperty, ""); }
            set { SetValue(DetailsProperty, value); }
        }

        [Column("RawServices")]
        public string RawServicesPerformed
        {
            get { return string.Join(",", ServicesPerformed); }
            set
            {
                if (value == null)
                {
                    ServicesPerformed = null;
                }
                else
                {
                    string[] guidStrings = value.Split(',');
                    var answer = new Guid[guidStrings.Length];
                    for (int i = 0; i < guidStrings.Length; i++)
                    {
                        Guid guid;
                        if (Guid.TryParse(guidStrings[i], out guid))
                        {
                            answer[i] = guid;
                        }
                    }

                    ServicesPerformed = answer;
                }
            }
        }

        public static readonly DataItemProperty ServicesProperty = DataItemProperty.Register(nameof(SyncItemMaintenanceRecordEntry.ServicesPerformed));

        [NotMapped]
        public Guid[] ServicesPerformed
        {
            get { return GetValue<Guid[]>(ServicesProperty); }
            set { SetValue(ServicesProperty, value); }
        }

        protected override Type GetSyncItemType()
        {
            return typeof(SyncItemMaintenanceRecordEntry);
        }
    }
}
