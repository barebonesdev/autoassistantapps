using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Helpers;
using System.Runtime.CompilerServices;
using ToolsPortable;
using AutoAssistantAppDataLibrary.Extensions;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ViewFuelViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemFuelEntry FuelEntry { get; private set; }

        public ViewFuelViewModel(MainScreenViewModel parent, ViewItemFuelEntry fuelEntry) : base(parent)
        {
            FuelEntry = fuelEntry;

            this.ListenToItem(fuelEntry.Identifier).Deleted += ViewFuelViewModel_Deleted;
        }

        public string MpgString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.MPG), delegate
                {
                    return AutoAssistantStringFormatter.FormatMpg(FuelEntry.MPG);
                });
            }
        }

        public string MilesString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.MilesSinceLast), delegate
                {
                    return AutoAssistantStringFormatter.FormatMilesWithText(FuelEntry.MilesSinceLast);
                });
            }
        }

        public string TotalMilesString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.Mileage), delegate
                {
                    return AutoAssistantStringFormatter.FormatMiles(FuelEntry.Mileage) + " total";
                });
            }
        }

        public string GallonsString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.Gallons), delegate
                {
                    return AutoAssistantStringFormatter.FormatGallonsWithText(FuelEntry.Gallons);
                });
            }
        }

        public string CostPerGallonString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.CostPerGallon), delegate
                {
                    return AutoAssistantStringFormatter.FormatPricePerGallonWithText(FuelEntry.CostPerGallon);
                });
            }
        }

        public string TotalCostString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.TotalCost), delegate
                {
                    return AutoAssistantStringFormatter.FormatCost(FuelEntry.TotalCost);
                });
            }
        }

        private string GetBindedValue(string fuelEntryPropertyName, Func<string> convert, [CallerMemberName]string propertyName = null)
        {
            return GetBindedValue(FuelEntry, fuelEntryPropertyName, convert, propertyName);
        }

        private void ViewFuelViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            MainScreenViewModel.ShowPopup(AddFuelViewModel.CreateForEdit(MainScreenViewModel, FuelEntry));
        }

        public async void Delete()
        {
            try
            {
                await MainScreenViewModel.DeleteFuel(FuelEntry.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
