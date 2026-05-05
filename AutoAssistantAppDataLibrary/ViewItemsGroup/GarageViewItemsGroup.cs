using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.ViewItemsGroup
{
    public class GarageViewItemsGroup : BaseAccountViewItemsGroup
    {
        private ViewItemGarage _garage;

        public MyObservableList<ViewItemVehicle> Vehicles { get; private set; }

        private GarageViewItemsGroup(Guid localAccountId, bool trackChanges) : base(localAccountId, trackChanges) { }

        public static Task<GarageViewItemsGroup> LoadAsync(Guid localAccountId, bool trackChanges = true)
        {
            return GetCachedOrLoad<GarageViewItemsGroup>(new GroupIdentity()
            {
                LocalAccountId = localAccountId
            }, trackChanges);
        }

        protected override async Task LoadBlockingAsync(AccountDataStore dataStore)
        {
            DataItemVehicle[] dataVehicles;

            using (await Locks.LockDataForReadAsync("VehiclesViewItemsGroup.LoadBlocking"))
            {
                dataVehicles = dataStore.TableVehicles.ToArray();
            }

            _garage = new ViewItems.ViewItemGarage(dataVehicles);

            Vehicles = _garage.Vehicles;
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            _garage.HandleDataChangedEvent(e);
        }
    }
}
