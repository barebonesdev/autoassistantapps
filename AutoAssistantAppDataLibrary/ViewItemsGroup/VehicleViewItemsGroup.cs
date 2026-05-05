using AutoAssistantAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace AutoAssistantAppDataLibrary.ViewItemsGroup
{
    public class VehicleViewItemsGroup : BaseVehicleChildrenViewItemsGroup
    {
        public event EventHandler OnChangesMade;
        public event EventHandler OnUpcomingServicesReset;

        public MyObservableList<ViewItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; } = new MyObservableList<ViewItemMaintenanceRecordEntry>();
        public MyObservableList<ViewItemMaintenanceScheduleItem> MaintenanceSchedule { get; private set; } = new MyObservableList<ViewItemMaintenanceScheduleItem>();

        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> UpcomingServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();

        public MyObservableList<ViewItemFuelEntry> Fuel { get; private set; }

        private VehicleViewItemsGroup(Guid localAccountId, bool trackChanges) : base(localAccountId, trackChanges) { }

        public static async Task<VehicleViewItemsGroup> LoadAsync(ViewItemVehicle vehicle, bool trackChanges = true)
        {
            var answer = await GetCachedOrLoad<VehicleViewItemsGroup>(vehicle, trackChanges);

            answer.Identity.Vehicle.RecalculateMileageStats(answer);
            answer.ResetUpcomingServices();

            return answer;
        }

        protected override async Task LoadBlockingAsync(AccountDataStore dataStore)
        {
            DataItemFuelEntry[] dataFuels;
            DataItemMaintenanceRecordEntry[] dataRecords;
            DataItemMaintenanceScheduleItem[] dataSchedules;
            var vehicle = Identity.Vehicle;

            using (await Locks.LockDataForReadAsync("FuelViewItemsGroup.LoadBlocking"))
            {
                dataFuels = dataStore.TableFuel.Where(i => i.VehicleIdentifier == vehicle.Identifier).ToArray();
                dataRecords = dataStore.TableMaintenanceRecords.Where(i => i.VehicleIdentifier == vehicle.Identifier).ToArray();
                dataSchedules = dataStore.TableMaintenanceSchedules.Where(i => i.VehicleIdentifier == vehicle.Identifier).ToArray();

                // Create the view objects
                Fuel = new MyObservableList<ViewItemFuelEntry>();

                foreach (var dataFuel in dataFuels)
                {
                    Fuel.InsertSorted(new ViewItemFuelEntry(dataFuel)
                    {
                        Vehicle = vehicle
                    });
                }

                foreach (var dataS in dataSchedules)
                {
                    MaintenanceSchedule.InsertSorted(new ViewItemMaintenanceScheduleItem(dataS)
                    {
                        Vehicle = vehicle
                    });
                }

                foreach (var dataRecord in dataRecords)
                {
                    MaintenanceRecords.InsertSorted(new ViewItemMaintenanceRecordEntry(dataRecord, this)
                    {
                        Vehicle = vehicle
                    });
                }

                RecalculateAll();
            }
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            bool changed = HandleDataChangedEventForVehicleChildren(Fuel, e.Fuel);
            changed = HandleDataChangedEventForVehicleChildren(MaintenanceRecords, e.MaintenanceRecords, creatorFunction: CreateViewItemRecord) || changed;
            changed = HandleDataChangedEventForVehicleChildren(MaintenanceSchedule, e.MaintenanceSchedule) || changed;

            if (changed)
            {
                RecalculateAll();
                Identity.Vehicle.RecalculateMileageStats(this);
                ResetUpcomingServices();
                OnChangesMade?.Invoke(this, new EventArgs());
            }
        }

        private ViewItemMaintenanceRecordEntry CreateViewItemRecord(DataItemMaintenanceRecordEntry dataItem)
        {
            return new ViewItemMaintenanceRecordEntry(dataItem, this);
        }

        /// <summary>
        /// Sets the mpg of the fuel entries across the range specified (indexes are inclusive)
        /// </summary>
        /// <param name="oldestIndex">Max index, like list.Count - 1</param>
        /// <param name="newestIndex">Like 0</param>
        private void AssignMpgOverRange(int oldestIndex, int newestIndex, decimal mpg)
        {
            for (int i = newestIndex; i <= oldestIndex; i++)
                Fuel[i].MPG = mpg;
        }

        private int FindNextFullFillupIndex(int fromIndex)
        {
            for (; fromIndex >= 0; fromIndex--)
                if (Fuel[fromIndex].PartialFill == false)
                    return fromIndex;

            return -1;
        }

        private void RecalculateAll()
        {
            RecalculateAllMilesSinceLast();
            RecalculateAllMpgs();
        }

        private IEnumerable<ViewItemMaintenanceRecordEntry> GetMaintenanceRecordsContaining(ViewItemMaintenanceScheduleItem item)
        {
            return MaintenanceRecords.Where(i => i.ServicesPerformed.Contains(item));
        }

        /// <summary>
        /// Need to make sure estimated mileage has been set BEFORE this gets called
        /// </summary>
        private void ResetUpcomingServices()
        {
            UpcomingServices.Clear();
            decimal estimatedMileage = Identity.Vehicle.EstimatedMileage;

            foreach (ViewItemMaintenanceScheduleItem item in MaintenanceSchedule)
            {
                DateTime date = Constants.NO_DATE;
                decimal mileage = Constants.NO_MILES;

                //find most recently done mileage and date
                foreach (var entry in GetMaintenanceRecordsContaining(item))
                {
                    if (entry.Date != Constants.NO_DATE && (date == Constants.NO_DATE || entry.Date > date))
                        date = entry.Date;

                    decimal mileageDoneAt = entry.GetMileageOrEstimatedMileage(MaintenanceRecords);

                    if (mileageDoneAt != Constants.NO_MILES && (mileage == Constants.NO_MILES || mileageDoneAt > mileage))
                        mileage = mileageDoneAt;
                }

                //if it's never been done before
                if (date == Constants.NO_DATE || mileage == Constants.NO_MILES)
                {
                    // Use mileage 0 as starting point
                    mileage = 0;

                    // Use vehicle purchase date (if available)
                    if (Identity.Vehicle.DatePurchased != Constants.NO_DATE && Identity.Vehicle.DatePurchased != SqlDate.MinValue)
                    {
                        date = Identity.Vehicle.DatePurchased;
                    }

                    // Otherwise estimate the date vehicle started at (falls back to Today)
                    else
                    {
                        date = Identity.Vehicle.EstimateDateOn(0);
                    }

                    //if overdue by mileage, or overdue by date
                    if ((item.MileageInterval != Constants.NO_MILES && estimatedMileage > item.MileageInterval) ||
                        (item.MonthInterval != Constants.NO_MONTHS && estimatedMileage > Identity.Vehicle.EstimateMilesDriven(TimeSpan.FromDays(30 * item.MonthInterval))))
                    {
                        //add an "OVERDUE" item
                        UpcomingServices.InsertSorted(new ViewItemUpcomingMaintenanceScheduleItem(
                            item,
                            Identity.Vehicle,
                            item.MileageInterval != Constants.NO_MILES ? item.MileageInterval : Constants.NO_MILES, //never done, so needed at its first mileage interval
                            item.MonthInterval != Constants.NO_MONTHS ? Identity.Vehicle.EstimateDateOn(Identity.Vehicle.EstimateMilesDriven(TimeSpan.FromDays(30 * item.MonthInterval))) : Constants.NO_DATE)); //impossible to calculate date without knowing when 

                        // And then we schedule the rest assuming if the user performed the overdue maintenance now
                        date = DateTime.Today;
                        mileage = Identity.Vehicle.EstimatedMileage;
                    }
                }






                int times = 0;

                //generate schedule up to 8 years in advance, or up to 100,000 miles in advance. Only allow 50 items of each schedule
                do
                {
                    //increment date
                    if (item.MonthInterval != Constants.NO_MONTHS && date != Constants.NO_DATE)
                        date = date.AddMonths(item.MonthInterval);

                    //increment mileage
                    if (item.MileageInterval != Constants.NO_MILES && mileage != Constants.NO_MILES)
                        mileage += item.MileageInterval;

                    UpcomingServices.InsertSorted(new ViewItemUpcomingMaintenanceScheduleItem(
                        item,
                        Identity.Vehicle,
                        item.MileageInterval != Constants.NO_MILES ? mileage : Constants.NO_MILES,
                        item.MonthInterval != Constants.NO_MONTHS ? date : Constants.NO_DATE));

                    times++;


                    //if we already did an overdue item, we jump the date and mileage to now
                    if (date < DateTime.Today)
                        date = DateTime.Today;
                    if (mileage < estimatedMileage)
                        mileage = estimatedMileage;

                } while (times < 50 && (item.MonthInterval != Constants.NO_MONTHS && date < DateTime.Today.AddYears(8)) || (item.MileageInterval != Constants.NO_MILES && mileage <= estimatedMileage + 100000));
            }

            OnUpcomingServicesReset?.Invoke(this, new EventArgs());
        }

        private void RecalculateAllMpgs()
        {
            int i = 0;
            while (i < Fuel.Count)
            {
                var curr = Fuel[i];
                if (curr.PartialFill
                    || curr.SkippedEnteringPreviousFillup
                    || curr.MilesSinceLast == Constants.NO_MILES
                    || curr.Gallons == Constants.NO_GALLONS
                    || i == Fuel.Count - 1)
                {
                    curr.MPG = Constants.NO_MPG;
                    i++;
                    continue;
                }

                int nextIndex;
                decimal mpg = CalculateMpg(curr.Mileage, curr.Gallons, i + 1, out nextIndex);

                AssignMpgOverRange(nextIndex - 1, i, mpg);

                i = nextIndex;
            }

            //if (Fuel.Count == 0)
            //{
            //    return;
            //}

            //if (Fuel.Count >= 1)
            //{
            //    Fuel.Last().MPG = Constants.NO_MPG;
            //    Fuel.Last().MilesSinceLast = Constants.NO_MILES;
            //}

            //if (Fuel.Count >= 2)
            //{
            //    int currIndex = Fuel.Count - 1;
            //    while (RecalculateMpgs(ref currIndex)) { }
            //}
        }

        private bool RecalculateMpgs(ref int currIndex)
        {
            decimal totalGallons = 0;
            int nextIndex = currIndex - 1;
            for (; nextIndex >= 0; nextIndex--)
            {
                var nextFuel = Fuel[nextIndex];

                // If we skipped a previous
                if (nextFuel.SkippedEnteringPreviousFillup)
                {
                    // This range is screwed
                    AssignMpgOverRange(currIndex - 1, nextIndex, Constants.NO_MPG);
                    currIndex = nextIndex;
                    return true;
                }

                totalGallons += nextFuel.Gallons;

                // If it's a full fill
                if (!nextFuel.PartialFill)
                {
                    // We're done with this range
                    decimal mpg;
                    if (totalGallons == 0)
                    {
                        mpg = Constants.NO_MPG;
                    }
                    else
                    {
                        mpg = (nextFuel.Mileage - Fuel[currIndex].Mileage) / totalGallons;
                    }

                    AssignMpgOverRange(currIndex - 1, nextIndex, mpg);
                    currIndex = nextIndex;
                    return true;
                }

                // Otherwise partial fill, keep going
            }

            // If reached here means we ended on a partial fill
            AssignMpgOverRange(currIndex - 1, 0, Constants.NO_MPG);
            return false;
        }

        private void RecalculateAllMilesSinceLast()
        {
            if (Fuel.Count > 0)
            {
                Fuel.Last().MilesSinceLast = Constants.NO_MILES;

                for (int i = Fuel.Count - 2; i >= 0; i--)
                {
                    var curr = Fuel[i];
                    var prev = Fuel[i + 1];

                    curr.MilesSinceLast = curr.Mileage - prev.Mileage;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mileageFilledUpAt"></param>
        /// <param name="gallonsFilledUp"></param>
        /// <param name="prevFuelIndex">The fillup immediately previous to this one (even if it's a partial fill or anything)</param>
        /// <param name="nextIndex"></param>
        /// <returns></returns>
        public decimal CalculateMpg(decimal mileageFilledUpAt, decimal gallonsFilledUp, int prevFuelIndex, out int nextIndex)
        {
            decimal totalGallons = gallonsFilledUp;
            for (int i = prevFuelIndex; i < Fuel.Count; i++)
            {
                var fuel = Fuel[i];

                // If full fillup
                if (!fuel.PartialFill)
                {
                    nextIndex = i;
                    if (totalGallons == 0)
                    {
                        return Constants.NO_MPG;
                    }
                    else
                    {
                        return (mileageFilledUpAt - fuel.Mileage) / totalGallons;
                    }
                }

                // If we skipped a previous fillup
                if (fuel.SkippedEnteringPreviousFillup)
                {
                    nextIndex = i;
                    // We can't calculate anything
                    return Constants.NO_MPG;
                }

                // Otherwise, add the gallons
                totalGallons += fuel.Gallons;
            }

            // If we reached here, means we can't know what the fillup was, we had a chain of partials.
            nextIndex = Fuel.Count;
            return Constants.NO_MPG;
        }

        /// <summary>
        /// Calculates the average MPG that the car got over the most recent X miles driven.
        /// </summary>
        /// <param name="amountOfMilesToInclude"></param>
        /// <returns></returns>
        public decimal CalculageMpgInLastMiles(decimal amountOfMilesToInclude)
        {
            // If there's none, then no MPG
            if (Fuel.Count == 0)
                return Constants.NO_MPG;

            decimal startMileage = Fuel.First().Mileage;

            decimal sumOfMiles = 0; // Sum of miles that have MPG data
            decimal sumOfGallons = 0;

            bool hasExistingCompleteSequences = false;

            decimal gallons = 0;
            ViewItemFuelEntry end = null;

            foreach (ViewItemFuelEntry curr in Fuel)
            {
                // If we're currently trying to complete a sequence
                if (end != null)
                {
                    // If this is a full fillup, then we've found a complete sequence from start to end
                    if (!curr.PartialFill)
                    {
                        decimal milesInThisSequence = end.Mileage - curr.Mileage;

                        if (startMileage - curr.Mileage > amountOfMilesToInclude)
                        {
                            decimal milesToIncludeFromThisSequence = amountOfMilesToInclude - (startMileage - end.Mileage);

                            // It could technically be 0, or even negative, if there were skips or partials
                            if (milesToIncludeFromThisSequence > 0)
                            {
                                // If there aren't any sequences before this, or if the MPG for this entire sequence matches the curr MPG for the combined before sequences
                                if (!hasExistingCompleteSequences ||
                                    (gallons != 0 && sumOfGallons != 0 && (milesInThisSequence / gallons) == (sumOfMiles / sumOfGallons)))
                                {
                                    // Then we might as well add the entire sequence since it won't imact the final result (and it will reduce rounding errors this way)
                                    sumOfMiles += milesInThisSequence;
                                    sumOfGallons += gallons;
                                }

                                else
                                {
                                    // Otherwise we need to only take a portion from this sequence 
                                    sumOfMiles += milesToIncludeFromThisSequence;
                                    sumOfGallons += gallons * (milesToIncludeFromThisSequence / milesInThisSequence);
                                }
                            }

                            break;
                        }

                        sumOfMiles += milesInThisSequence;
                        sumOfGallons += gallons;

                        // Completed the sequence
                        end = null;

                        hasExistingCompleteSequences = true;
                    }

                    // Otherwise this is a partial, and if it's also skipped we can't complete the sequence
                    else if (curr.SkippedEnteringPreviousFillup)
                        end = null; // Can't complete the sequence

                    // Otherwise include its gallons and continue the sequence
                    else
                        gallons += curr.Gallons;
                }

                // Now if we weren't trying to complete a sequence OR we just completed a sequence (previous code set end to null)
                if (end == null)
                {
                    // If we have a full fillup that didn't skip previous, we can attempt to start a new sequence
                    if (!curr.PartialFill && !curr.SkippedEnteringPreviousFillup)
                    {
                        end = curr;
                        gallons = curr.Gallons;
                    }
                }
            }

            if (sumOfGallons <= 0 || sumOfMiles < 0)
                return Constants.NO_MPG;

            return sumOfMiles / sumOfGallons;
        }
    }
}
