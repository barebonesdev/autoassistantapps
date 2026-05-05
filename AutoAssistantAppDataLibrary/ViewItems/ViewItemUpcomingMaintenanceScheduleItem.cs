using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemUpcomingMaintenanceScheduleItem : BindableBase, IComparable<ViewItemUpcomingMaintenanceScheduleItem>, IComparable
    {
        public ViewItemMaintenanceScheduleItem ScheduleItem { get; private set; }

        private decimal _milesNeededAt;
        public decimal MilesNeededAt
        {
            get { return _milesNeededAt; }
            private set { SetProperty(ref _milesNeededAt, value, nameof(MilesNeededAt)); }
        }

        private DateTime _dateNeededAt;
        public DateTime DateNeededAt
        {
            get { return _dateNeededAt; }
            private set { SetProperty(ref _dateNeededAt, value, nameof(DateNeededAt)); }
        }

        private bool _isDateSooner;
        public bool IsDateSooner
        {
            get { return _isDateSooner; }
            private set { SetProperty(ref _isDateSooner, value, nameof(IsDateSooner)); }
        }

        private string _counter;
        public string Counter
        {
            get { return _counter; }
            private set { SetProperty(ref _counter, value, nameof(Counter)); }
        }

        private string _counterType;
        public string CounterType
        {
            get { return _counterType; }
            private set { SetProperty(ref _counterType, value, nameof(CounterType)); }
        }

        private string _subtitle;
        public string Subtitle
        {
            get { return _subtitle; }
            private set { SetProperty(ref _subtitle, value, nameof(Subtitle)); }
        }

        public ViewItemVehicle Vehicle { get; private set; }

        public ViewItemUpcomingMaintenanceScheduleItem(
            ViewItemMaintenanceScheduleItem scheduleItem,
            ViewItemVehicle vehicle,
            decimal milesNeededAt,
            DateTime dateNeededAt)
        {
            MilesNeededAt = milesNeededAt;
            ScheduleItem = scheduleItem;
            DateNeededAt = dateNeededAt;
            Vehicle = vehicle;

            InitializeUseDate(vehicle);
            InitializeCounter(vehicle);
            InitializeSubtitle();
        }

        public void Initialize(ViewItemUpcomingMaintenanceScheduleItem other)
        {
            MilesNeededAt = other.MilesNeededAt;
            DateNeededAt = other.DateNeededAt;
            IsDateSooner = other.IsDateSooner;
            Counter = other.Counter;
            CounterType = other.CounterType;
            Subtitle = other.Subtitle;
        }

        private void InitializeCounter(ViewItemVehicle vehicle)
        {
            if (IsDateSooner)
            {
                var today = DateTime.Today;
                int days = (DateNeededAt.Date - today).Days;
                int months = DateTools.DifferenceInMonths(DateNeededAt.Date, today);

                if (Math.Abs(days) >= 60 && Math.Abs(months) >= 2)
                {
                    Counter = months.ToString();
                    CounterType = "months";
                }

                else
                {
                    Counter = days.ToString();

                    if (days == 1)
                        CounterType = "day";
                    else
                        CounterType = "days";
                }
            }

            else
            {
                decimal miles = MilesNeededAt - vehicle.EstimatedMileage;

                if (miles >= 3000 || miles <= -3000)
                    Counter = Math.Round(miles / 1000, 0, MidpointRounding.AwayFromZero).ToString("N0") + "K";
                else if (miles >= 1000 || miles <= -1000)
                    Counter = Math.Round(miles / 1000, 1, MidpointRounding.AwayFromZero).ToString("N1") + "K";
                else
                    Counter = miles.ToString("N0");

                CounterType = "miles";
            }
        }

        private void InitializeSubtitle()
        {
            var answer = "";

            if (ScheduleItem.EstimatedCost != Constants.NO_COST)
                answer = ScheduleItem.EstimatedCost.ToString("C") + " - ";

            if (MilesNeededAt != Constants.NO_MILES)
            {
                answer += MilesNeededAt.ToString("N0") + " miles";

                if (DateNeededAt != Constants.NO_DATE)
                {
                    answer += " or " + DateNeededAt.ToString("d");
                }
            }
            else if (DateNeededAt != Constants.NO_DATE)
            {
                answer += DateNeededAt.ToString("d");
            }
            else
            {
                answer += "Neither mileage nor month interval specified";
            }

            Subtitle = answer;
        }

        private void InitializeUseDate(ViewItemVehicle vehicle)
        {
            if (MilesNeededAt == Constants.NO_MILES)
                IsDateSooner = true;

            else if (DateNeededAt == Constants.NO_DATE)
                IsDateSooner = false;

            else if (MilesNeededAt - vehicle.EstimatedMileage <= (DateNeededAt.Date - DateTime.Today).Days * vehicle.EstimatedMilesPerDay)
                IsDateSooner = false;

            else
                IsDateSooner = true;
        }

        public int CompareTo(ViewItemUpcomingMaintenanceScheduleItem other)
        {
            //TODO - support NoDate or NoMiles

            //simple case where both date and mileage come sooner
            if (DateNeededAt < other.DateNeededAt && MilesNeededAt < other.MilesNeededAt)
                return -1;

            //simple case where date OR mileage are same
            if (DateNeededAt == other.DateNeededAt || MilesNeededAt == other.MilesNeededAt)
            {
                //mileage was the same
                if (DateNeededAt < other.DateNeededAt)
                    return -1;

                if (DateNeededAt > other.DateNeededAt)
                    return 1;


                //date was the same
                if (MilesNeededAt < other.MilesNeededAt)
                    return -1;

                if (MilesNeededAt > other.MilesNeededAt)
                    return 1;


                //both were the same
                return 0;
            }

            //simple case where date and mileage are later
            if (DateNeededAt > other.DateNeededAt && MilesNeededAt > other.MilesNeededAt)
                return 1;



            //now all that's left is two cases
            // - DateNeededAt < other.DateNeededAt && Mileage > other.Mileage
            // - DateNeededAt > other.DateNeededAt && Mileage < other.Mileage

            //get an estimate of how many miles would be driven till the service is due
            decimal milesTillDate = Vehicle.EstimateMilesDriven(DateNeededAt - DateTime.Today);

            //now see how many miles it is till the car reaches the MilesNeededAt
            decimal milesTillNeeded = MilesNeededAt - Vehicle.EstimatedMileage;

            decimal otherMilesTillDate = Vehicle.EstimateMilesDriven(other.DateNeededAt - DateTime.Today);
            decimal otherMilesTillNeeded = other.MilesNeededAt - Vehicle.EstimatedMileage;


            //if the min of the date or miles comes first
            if (Math.Min(milesTillDate, milesTillNeeded) < Math.Min(otherMilesTillDate, otherMilesTillNeeded))
                return -1;

            //if the min of the date or miles matches the other's min
            if (Math.Min(milesTillDate, milesTillNeeded) == Math.Min(otherMilesTillDate, otherMilesTillNeeded))
            {
                //compare their maxes
                return Math.Max(milesTillDate, milesTillNeeded).CompareTo(Math.Max(otherMilesTillNeeded, otherMilesTillDate));
            }

            //the min of the date or miles came later
            return 1;
        }

        public int CompareTo(object obj)
        {
            if (obj is ViewItemUpcomingMaintenanceScheduleItem)
            {
                return CompareTo(obj as ViewItemUpcomingMaintenanceScheduleItem);
            }

            return 0;
        }
    }
}
