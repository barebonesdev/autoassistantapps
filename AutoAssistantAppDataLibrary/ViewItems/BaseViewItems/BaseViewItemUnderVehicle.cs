using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemUnderVehicle : BaseViewItem
    {
        public BaseViewItemUnderVehicle(Guid identifier) : base(identifier) { }
        public BaseViewItemUnderVehicle(BaseDataItemUnderVehicle dataItem) : base(dataItem) { }

        /// <summary>
        /// This needs to be set by the ViewItemsGroup that constructs the object. Vehicle should never change.
        /// </summary>
        public ViewItemVehicle Vehicle { get; set; }
    }
}
