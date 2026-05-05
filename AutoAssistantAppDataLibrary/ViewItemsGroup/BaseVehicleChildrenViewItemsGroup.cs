using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewItemsGroup
{
    public abstract class BaseVehicleChildrenViewItemsGroup : BaseAccountViewItemsGroup
    {
        protected class VehicleChildrenGroupIdentity : GroupIdentity
        {
            public ViewItemVehicle Vehicle { get; set; }

            public Type ViewGroupType { get; set; }

            public override bool Equals(GroupIdentity other)
            {
                // We compare reference to vehicle, since children use the vehicle as their parent
                return base.Equals(other) && Vehicle == (other as VehicleChildrenGroupIdentity).Vehicle && ViewGroupType == (other as VehicleChildrenGroupIdentity).ViewGroupType;
            }
        }

        protected new VehicleChildrenGroupIdentity Identity { get { return base.Identity as VehicleChildrenGroupIdentity; } }

        public BaseVehicleChildrenViewItemsGroup(Guid localAccountId, bool trackChanges = true) : base(localAccountId, trackChanges)
        {
        }

        protected static Task<T> GetCachedOrLoad<T>(ViewItemVehicle vehicle, bool trackChanges)
            where T : BaseAccountViewItemsGroup
        {
            return GetCachedOrLoad<T>(new VehicleChildrenGroupIdentity()
            {
                LocalAccountId = vehicle.Account.LocalAccountId,
                Vehicle = vehicle,
                ViewGroupType = typeof(T)
            }, trackChanges);
        }

        /// <summary>
        /// Handles changes for vehicle children which don't have any children of their own
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="list"></param>
        /// <param name="scopedChanges"></param>
        protected bool HandleDataChangedEventForVehicleChildren<V, D>(MyObservableList<V> list, DataChangedEvent.ScopedDataChangedEvent<D> scopedChanges, Func<D, V> creatorFunction = null)
            where V : BaseViewItemUnderVehicle
            where D : BaseDataItemUnderVehicle, new()
        {
            // Remove items that were deleted
            bool changed = list.RemoveWhere(i => scopedChanges.DeletedItems.Contains(i.Identifier));

            // Look through edited items
            foreach (var edited in scopedChanges.EditedItems)
            {
                var matched = list.FirstOrDefault(i => i.Identifier == edited.Identifier);

                // Note: Since we don't allow changing parent vehicle, edited item should always remain child
                // And thus we don't have to worry about the case where we failed to find an edited match that SHOULD become a child

                if (matched != null)
                {
                    // Note: If we start having sub-children, we'll have to switch this to call HandleDataChangedEvent instead
                    matched.PopulateFromDataItem(edited);

                    // Note: Don't have to assign vehicle parent since it never changes

                    // And then add/remove (a.k.a. resort)
                    list.Remove(matched);
                    list.InsertSorted(matched);

                    changed = true;
                }
            }

            // Note: Don't have to worry about removing items that were edited out of this parent vehicle

            // Add new items
            foreach (var newItem in scopedChanges.NewItems.Where(i => i.VehicleIdentifier == Identity.Vehicle.Identifier))
            {
                V viewItem;
                if (creatorFunction == null)
                {
                    viewItem = (V)Activator.CreateInstance(typeof(V), newItem);
                }
                else
                {
                    viewItem = creatorFunction(newItem);
                }
                viewItem.Vehicle = Identity.Vehicle;
                list.InsertSorted(viewItem);

                changed = true;
            }

            return changed;
        }
    }
}
