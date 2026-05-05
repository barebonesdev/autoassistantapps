using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    public class DataChangedEvent
    {
        public Guid LocalAccountId { get; private set; }

        public class ScopedDataChangedEvent<T> where T : BaseDataItem, new()
        {
            public List<T> NewItems { get; private set; } = new List<T>();

            public List<T> EditedItems { get; private set; } = new List<T>();

            public DeletedItems DeletedItems { get; private set; } = new DeletedItems();

            public bool HasChanges()
            {
                return NewItems.Count > 0 || EditedItems.Count > 0 || DeletedItems.Any();
            }

            public void Merge(ScopedDataChangedEvent<T> newerEvent)
            {
                // Previously new or edited items might have been deleted, so make sure they're removed
                foreach (var del in newerEvent.DeletedItems)
                {
                    // Try remove from new items
                    if (!RemoveMatching(NewItems, del))
                    {
                        // Otherwise try remove from edited items
                        RemoveMatching(EditedItems, del);
                    }
                }

                // Add the deletes
                DeletedItems.Merge(newerEvent.DeletedItems);

                // Merge the edits
                foreach (var edited in newerEvent.EditedItems)
                {
                    ProcessMergingEditedItem(edited);
                }

                // And then add the new items (never will have conflicts since item can only be added once)
                NewItems.AddRange(newerEvent.NewItems);
            }

            private void ProcessMergingEditedItem(T editedItem)
            {
                // If we already have this as a new item, then the item replaces it
                if (MergeIntoList(NewItems, editedItem))
                    return;

                // Otherwise if we already have this as an edited item, then the new item replaces it
                if (MergeIntoList(EditedItems, editedItem))
                    return;

                // Otherwise, we add it to edited items
                EditedItems.Add(editedItem);
            }

            private static bool MergeIntoList(List<T> listToMergeInto, T itemToMerge)
            {
                for (int i = 0; i < listToMergeInto.Count; i++)
                {
                    // If we already have this item
                    if (listToMergeInto[i].Identifier == itemToMerge.Identifier)
                    {
                        // Replace it with the item to merge
                        listToMergeInto[i] = itemToMerge;
                        return true;
                    }
                }

                return false;
            }

            private static bool RemoveMatching(List<T> listToRemoveFrom, Guid identifier)
            {
                for (int i = 0; i < listToRemoveFrom.Count; i++)
                    if (listToRemoveFrom[i].Identifier == identifier)
                    {
                        listToRemoveFrom.RemoveAt(i);
                        return true;
                    }

                return false;
            }
        }

        public ScopedDataChangedEvent<DataItemVehicle> Vehicles { get; private set; } = new ScopedDataChangedEvent<DataItemVehicle>();
        public ScopedDataChangedEvent<DataItemFuelEntry> Fuel { get; private set; } = new ScopedDataChangedEvent<DataItemFuelEntry>();
        public ScopedDataChangedEvent<DataItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; } = new ScopedDataChangedEvent<DataItemMaintenanceRecordEntry>();
        public ScopedDataChangedEvent<DataItemMaintenanceScheduleItem> MaintenanceSchedule { get; private set; } = new ScopedDataChangedEvent<DataItemMaintenanceScheduleItem>();

        public DataChangedEvent(Guid localAccountId)
        {
            LocalAccountId = localAccountId;
        }

        public void Merge(DataChangedEvent newerEvent)
        {
            Vehicles.Merge(newerEvent.Vehicles);
            Fuel.Merge(newerEvent.Fuel);
            MaintenanceRecords.Merge(newerEvent.MaintenanceRecords);
            MaintenanceSchedule.Merge(newerEvent.MaintenanceSchedule);
        }
    }
}
