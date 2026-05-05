using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using AutoAssistantAppDataLibraryTests.FakeData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibraryTests.Helpers
{
    public abstract class BaseTest
    {
        public MainWindowViewModel MainWindowViewModel
        {
            get { return AutoAssistantTestApp.Current.MainWindowViewModel; }
        }

        public MainScreenViewModel MainScreenViewModel
        {
            get { return MainWindowViewModel?.Content as MainScreenViewModel; }
        }

        protected async Task<ViewItemFuelEntry> AddFuelAsync(decimal mileage, decimal gallons, decimal totalCost, decimal expectedMpg, bool isPartial = false, bool skippedPrev = false)
        {
            var fuelViewModel = MainScreenViewModel.Content as FuelViewModel;
            if (fuelViewModel == null)
            {
                throw new NullReferenceException("You must be on the fuel page first");
            }

            await fuelViewModel.LoadAsync();

            fuelViewModel.AddFuel();

            var addFuelViewModel = MainScreenViewModel.Popups.First() as AddFuelViewModel;
            await addFuelViewModel.LoadAsync();
            addFuelViewModel.Mileage = mileage;
            addFuelViewModel.TotalCost = totalCost;
            addFuelViewModel.Gallons = gallons;
            if (isPartial)
            {
                addFuelViewModel.PartialFill = true;
            }
            if (skippedPrev)
            {
                addFuelViewModel.SkippedEnteringPreviousFillup = true;
            }

            // Ensure MPG estimate is correct
            Assert.AreEqual(expectedMpg, addFuelViewModel.MPG, "Preview MPG was incorrect");
            int countBefore = fuelViewModel.FuelEntries.Count;

            addFuelViewModel.Save();

            await ChangeHelper.WaitTillDataChangedCompleted(fuelViewModel._fuelViewItemsGroup);

            // Ensure fuel was added
            Assert.AreEqual(countBefore + 1, fuelViewModel.FuelEntries.Count);

            var fuel = fuelViewModel.FuelEntries.First();

            Assert.AreEqual(mileage, fuel.Mileage);
            Assert.AreEqual(gallons, fuel.Gallons);
            Assert.AreEqual(Math.Round(totalCost / gallons, 3), fuel.CostPerGallon);

            // Ensure MPG is correct
            Assert.AreEqual(expectedMpg, fuel.MPG, "Actual MPG was incorrect");

            // Make sure miles since last is correct
            if (fuelViewModel.FuelEntries.Count > 1)
            {
                Assert.AreEqual(fuel.Mileage - fuelViewModel.FuelEntries[1].Mileage, fuel.MilesSinceLast);
            }
            else
            {
                Assert.AreEqual(Constants.NO_MILES, fuel.MilesSinceLast);
            }

            return fuel;
        }
    }
}
