using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantAppDataLibrary.Helpers;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemMaintenanceScheduleItem : BaseViewItemUnderVehicle, IComparable<ViewItemMaintenanceScheduleItem>
    {
        public ViewItemMaintenanceScheduleItem(Guid identifier) : base(identifier) { }
        public ViewItemMaintenanceScheduleItem(DataItemMaintenanceScheduleItem dataItem) : base(dataItem) { }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value, nameof(Title)); }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        private decimal _mileageInterval;
        public decimal MileageInterval
        {
            get { return _mileageInterval; }
            set { SetProperty(ref _mileageInterval, value, nameof(MileageInterval)); }
        }

        private short _monthInterval = Constants.NO_MONTHS;
        public short MonthInterval
        {
            get { return _monthInterval; }
            set { SetProperty(ref _monthInterval, value, nameof(MonthInterval)); }
        }

        private decimal _estimatedCost;
        public decimal EstimatedCost
        {
            get { return _estimatedCost; }
            set { SetProperty(ref _estimatedCost, value, nameof(EstimatedCost)); }
        }

        private string _subtitle;
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value, nameof(Subtitle)); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            var i = (DataItemMaintenanceScheduleItem)dataItem;

            Title = i.Title;
            Details = i.Details;
            MileageInterval = i.MileageInterval;
            MonthInterval = i.MonthInterval;
            EstimatedCost = i.EstimatedCost;

            string subtitle = "Every ";
            if (MileageInterval != Constants.NO_MILES)
            {
                subtitle += AutoAssistantStringFormatter.FormatMilesWithText(MileageInterval);
                if (MonthInterval != Constants.NO_MONTHS)
                {
                    subtitle += " or ";
                }
            }
            if (MonthInterval != Constants.NO_MONTHS)
            {
                if (MonthInterval >= 24 && MonthInterval % 12 == 0)
                {
                    subtitle += (MonthInterval / 12) + " years";
                }
                else
                {
                    subtitle += MonthInterval + " months";
                }
            }

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
            var editedItem = e.MaintenanceSchedule.EditedItems.FirstOrDefault(i => i.Identifier == Identifier);
            if (editedItem != null)
            {
                PopulateFromDataItem(editedItem);
                return true;
            }

            return false;
        }

        /// <summary>
        /// If mileage interval isn't specified, falls back to using month interval with assumption of 15,000 miles per year
        /// </summary>
        /// <returns></returns>
        internal decimal GetMileageIntervalForCompare()
        {
            if (MileageInterval != Constants.NO_MILES)
            {
                return MileageInterval;
            }

            return MonthInterval * (15000 / 12);
        }

        internal bool IsMissingAnInterval()
        {
            return MonthInterval == Constants.NO_MONTHS || MileageInterval == Constants.NO_MILES;
        }

        public int CompareTo(ViewItemMaintenanceScheduleItem other)
        {
            int answer = GetMileageIntervalForCompare().CompareTo(other.GetMileageIntervalForCompare());

            if (answer == 0)
            {
                if (this.IsMissingAnInterval())
                {
                    if (!other.IsMissingAnInterval())
                    {
                        // This goes later since it's less complete
                        return 1;
                    }
                    else
                    {
                        // Both missing an interval, can't do much
                    }
                }
                else if (other.IsMissingAnInterval())
                {
                    // This goes earlier since it's more complete
                    return -1;
                }
                else
                {
                    // Compare by month (since we were comparing by mileage before
                    answer = this.MonthInterval.CompareTo(other.MonthInterval);
                }
            }

            if (answer == 0)
            {
                return base.CompareTo(other);
            }

            return answer;
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemMaintenanceScheduleItem otherSchedule)
            {
                return CompareTo(otherSchedule);
            }

            return base.CompareTo(other);
        }
    }
}
