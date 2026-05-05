using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.Helpers;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemMaintenanceRecordEntry : BaseViewItemUnderVehicle, IComparable<ViewItemMaintenanceRecordEntry>
    {
        public MyObservableList<ViewItemMaintenanceScheduleItem> AllScheduleItems { get; private set; }
        private VehicleViewItemsGroup _vehicleViewItemsGroup;

        /// <summary>
        /// Assumes VehicleViewItemsGroup has already loaded all of the schedules
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="vehicleViewItemsGroup"></param>
        public ViewItemMaintenanceRecordEntry(DataItemMaintenanceRecordEntry dataItem, VehicleViewItemsGroup vehicleViewItemsGroup) : base(dataItem)
        {
            // Hold a reference to the view items group so it doesn't dispose
            _vehicleViewItemsGroup = vehicleViewItemsGroup;
            AllScheduleItems = vehicleViewItemsGroup.MaintenanceSchedule;

            _vehicleViewItemsGroup.OnChangesMade += new WeakEventHandler<EventArgs>(_vehicleViewItemsGroup_OnChangesMade).Handler;

            UpdateServicesPerformed();
        }

        private void _vehicleViewItemsGroup_OnChangesMade(object sender, EventArgs e)
        {
            // Every time changes were made to maintenance items (schedules or records), we'll update the services performed.
            // That's because we have to watch both the services (if a service gets deleted) and the item itself. This is the
            // easiest implementation.
            UpdateServicesPerformed();
        }

        private void UpdateServicesPerformed()
        {
            ServicesPerformed.MakeListLike(AllScheduleItems.Where(s => (DataItem as DataItemMaintenanceRecordEntry).ServicesPerformed.Contains(s.Identifier)).ToList());

            string secondarySubtitle = string.Join(", ", ServicesPerformed.Select(x => x.Title.Trim()));
            if (secondarySubtitle.Length == 0)
            {
                secondarySubtitle = Details.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(Details))
            {
                secondarySubtitle += ". " + Details.Trim();
            }
            SecondarySubtitle = secondarySubtitle;
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value, nameof(Title)); }
        }

        private string _subtitle;
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value, nameof(Subtitle)); }
        }

        private string _secondarySubtitle;
        public string SecondarySubtitle
        {
            get { return _secondarySubtitle; }
            set { SetProperty(ref _secondarySubtitle, value, nameof(SecondarySubtitle)); }
        }

        private string _doneBy;
        public string DoneBy
        {
            get { return _doneBy; }
            set { SetProperty(ref _doneBy, value, nameof(DoneBy)); }
        }

        private decimal _mileage;
        public decimal Mileage
        {
            get { return _mileage; }
            set { SetProperty(ref _mileage, value, nameof(Mileage)); }
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); }
        }

        private decimal _cost;
        public decimal Cost
        {
            get { return _cost; }
            set { SetProperty(ref _cost, value, nameof(Cost)); }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        public MyObservableList<ViewItemMaintenanceScheduleItem> ServicesPerformed { get; private set; } = new MyObservableList<ViewItemMaintenanceScheduleItem>();

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            var i = (DataItemMaintenanceRecordEntry)dataItem;

            Title = i.Title;
            DoneBy = i.DoneBy;
            Mileage = i.Mileage;
            Date = DateTime.SpecifyKind(i.Date, DateTimeKind.Local);
            Cost = i.Cost;
            Details = i.Details;

            string subtitle = "";

            if (Cost != Constants.NO_COST)
            {
                subtitle = AutoAssistantStringFormatter.FormatCost(Cost) + " - ";
            }

            if (Mileage != Constants.NO_MILES)
            {
                subtitle += AutoAssistantStringFormatter.FormatMilesWithText(Mileage) + " on ";
            }

            subtitle += Date.ToString("d");

            Subtitle = subtitle;
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
            var editedItem = e.MaintenanceRecords.EditedItems.FirstOrDefault(i => i.Identifier == Identifier);
            if (editedItem != null)
            {
                PopulateFromDataItem(editedItem);
                return true;
            }

            return false;
        }

        public int CompareTo(ViewItemMaintenanceRecordEntry other)
        {
            int comp = 0;

            if (Mileage != Constants.NO_MILES && other.Mileage != Constants.NO_MILES)
            {
                // We want oldest items FIRST, so reversing compare order
                comp = other.Mileage.CompareTo(Mileage);
                if (comp != 0)
                {
                    return comp;
                }
            }

            if (Date != Constants.NO_DATE && other.Date != Constants.NO_DATE)
            {
                comp = other.Date.CompareTo(Date);
                if (comp != 0)
                {
                    return comp;
                }
            }

            return base.CompareTo(other);
        }

        internal decimal GetMileageOrEstimatedMileage(IEnumerable<ViewItemMaintenanceRecordEntry> allRecords)
        {
            if (Mileage != Constants.NO_MILES)
            {
                return Mileage;
            }

            if (Date == Constants.NO_DATE)
            {
                return Constants.NO_MILES;
            }

            ViewItemMaintenanceRecordEntry closestBefore = null;
            ViewItemMaintenanceRecordEntry closestAfter = null;

            foreach (var record in allRecords)
            {
                if (record.Mileage != Constants.NO_MILES)
                {
                    if (record.Date.Date == this.Date.Date)
                    {
                        return record.Mileage;
                    }

                    if (record.Date.Date < this.Date.Date && (closestBefore == null || record.Date.Date > closestBefore.Date.Date))
                    {
                        closestBefore = record;
                    }

                    if (record.Date.Date > this.Date.Date && (closestAfter == null || record.Date.Date < closestAfter.Date.Date))
                    {
                        closestAfter = record;
                    }
                }
            }

            if (closestBefore != null && closestAfter != null)
            {
                double daysBetween = (closestAfter.Date.Date - closestBefore.Date.Date).TotalDays;

                decimal mileageBetween = closestAfter.Mileage - closestBefore.Mileage;

                double percentToClosestAfter = (this.Date.Date - closestBefore.Date.Date).TotalDays / daysBetween;

                return closestBefore.Mileage + mileageBetween * (decimal)percentToClosestAfter;
            }

            if (closestAfter != null)
            {
                var estimatedDateBorn = Vehicle.GetEstimatedDateBorn();
                if (estimatedDateBorn != null)
                {
                    // If the closest date is in fact before the estimated born date
                    if (closestAfter.Date.Date < estimatedDateBorn.Value.Date)
                    {
                        // Can't use that, so just return 0 miles
                        return 0;
                    }

                    double daysBetween = (closestAfter.Date.Date - estimatedDateBorn.Value.Date).TotalDays;

                    decimal mileageBetween = closestAfter.Mileage;

                    double percentToClosestAfter = (this.Date.Date - estimatedDateBorn.Value.Date).TotalDays / daysBetween;

                    return mileageBetween * (decimal)percentToClosestAfter;
                }
                else
                {
                    return 0;
                }
            }

            return Vehicle.EstimatedMileageOn(Date);
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemMaintenanceRecordEntry)
            {
                return CompareTo(other as ViewItemMaintenanceRecordEntry);
            }

            return base.CompareTo(other);
        }
    }
}
