using AutoAssistantAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.DataLayer;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemGarage : BaseViewItemWithoutData
    {
        public MyObservableList<ViewItemVehicle> Vehicles { get; private set; }

        public ViewItemGarage(IEnumerable<DataItemVehicle> dataVehicles)
        {
            Vehicles = new MyObservableList<ViewItemVehicle>();

            AddVehicles(dataVehicles);
        }

        protected override IEnumerable<BaseViewItemWithoutData> EnumerateAllChildrenThatHaveChildren()
        {
            return Vehicles;
        }

        internal override bool HandleUpdatingSelfAndDescendants(DataChangedEvent e)
        {
            bool changed = false;

            // Nothing to update on self

            // Edit all children
            foreach (var v in Vehicles)
            {
                if (v.HandleUpdatingSelfAndDescendants(e))
                {
                    changed = true;
                }
            }

            return changed;
        }

        internal override bool HandleModifyingChildren(DataChangedEvent e)
        {
            return HandleModifyingChildren<ViewItemVehicle, DataItemVehicle>(
                list: Vehicles,
                e: e.Vehicles,
                isChild: d => true,
                createChild: (d) => { return new ViewItemVehicle(d); });
        }

        private void AddVehicles(IEnumerable<DataItemVehicle> dataVehicles)
        {
            foreach (var d in dataVehicles)
            {
                AddVehicle(d);
            }
        }

        private void AddVehicle(DataItemVehicle dataVehicle)
        {
            Vehicles.InsertSorted(new ViewItemVehicle(dataVehicle));
        }
    }
}
