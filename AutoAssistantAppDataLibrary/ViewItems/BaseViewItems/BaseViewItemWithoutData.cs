using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithoutData : BindableBase
    {
        internal bool HandleDataChangedEvent(DataChangedEvent e)
        {
            bool changed = false;

            // Apply edits to self and descendants
            if (HandleUpdatingSelfAndDescendants(e))
            {
                changed = true;
            }

            // Modify the children (adds/removes)
            if (HandleModifyingChildrenRecursively(e))
            {
                changed = true;
            }

            return changed;
        }

        internal abstract bool HandleUpdatingSelfAndDescendants(DataChangedEvent e);

        internal bool HandleModifyingChildrenRecursively(DataChangedEvent e)
        {
            bool changed = false;

            // Make sure children are in right place (and add new/existing)
            if (HandleModifyingChildren(e))
            {
                changed = true;
            }

            // And do same for children recursively
            foreach (var child in EnumerateAllChildrenThatHaveChildren())
            {
                if (child.HandleModifyingChildrenRecursively(e))
                {
                    changed = true;
                }
            }

            return changed;
        }

        internal static bool HandleModifyingChildren<V, D>(MyObservableList<V> list, DataChangedEvent.ScopedDataChangedEvent<D> e, Func<D, bool> isChild, Func<D, V> createChild)
            where V : BaseViewItem
            where D : BaseDataItem, new()
        {
            bool changed = false;

            List<V> toRemove = new List<V>();
            List<V> toReSort = new List<V>();

            foreach (V child in list)
            {
                // If it was deleted, then we mark it for remove
                if (e.DeletedItems.Contains(child.Identifier))
                    toRemove.Add(child);

                else
                {
                    D edited = e.EditedItems.OfType<D>().FirstOrDefault(i => i.Identifier == child.Identifier);

                    // If it was edited
                    if (edited != null)
                    {
                        // We'll need to re-sort it
                        toReSort.Add(child);
                    }
                }
            }

            if (toRemove.Count > 0 || toReSort.Count > 0)
                changed = true;

            // Now remove all that need removing
            foreach (V item in toRemove)
                list.Remove(item);

            // And re-sort all that need re-sorting
            foreach (V item in toReSort)
            {
                // First remove
                list.Remove(item);

                // Then re-add
                list.InsertSorted(item);
            }

            // NOTE: Since Auto Assistant doesn't allow changing vehicle of items, we don't need any logic
            // for when an item is moved to a different parent

            // And now add the new items
            if (AddChildren(list, e.NewItems.Where(isChild), createChild))
            {
                changed = true;
            }

            return changed;
        }

        private static bool AddChildren<V, D>(MyObservableList<V> list, IEnumerable<D> dataItems, Func<D, V> createChild)
            where V : BaseViewItem
            where D : BaseDataItem, new()
        {
            int before = list.Count;
            list.InsertSorted(dataItems.Select(i => createChild(i)), trackChanges: false);
            return list.Count > before;
        }

        internal abstract bool HandleModifyingChildren(DataChangedEvent e);

        protected abstract IEnumerable<BaseViewItemWithoutData> EnumerateAllChildrenThatHaveChildren();
    }
}
