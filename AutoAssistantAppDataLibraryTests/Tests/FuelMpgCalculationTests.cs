using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoAssistantAppDataLibraryTests.FakeData;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using AutoAssistantAppDataLibraryTests.Helpers;
using AutoAssistantAppDataLibrary;

namespace AutoAssistantAppDataLibraryTests.Tests
{
    [TestClass]
    public class FuelMpgCalculationTests : BaseTest
    {
        [TestMethod]
        public async Task TestFuels()
        {
            await AutoAssistantTestApp.InitializeWithBlankVehicleAsync();

            MainScreenViewModel.SelectedItem = AutoAssistantAppDataLibrary.DataLayer.NavigationManager.MainMenuSelections.Fuel;

            var fuelViewModel = await ChangeHelper.GetContentAsync<FuelViewModel>(MainScreenViewModel);
            await fuelViewModel.LoadAsync();

            await AddFuelAsync(
                mileage: 100,
                gallons: 10,
                totalCost: 35,
                expectedMpg: Constants.NO_MPG);

            await AddFuelAsync(
                mileage: 200,
                gallons: 5,
                totalCost: 16,
                expectedMpg: 20);

            // Add a third fuel with different MPG
            await AddFuelAsync(
                mileage: 315,
                gallons: 6.753m,
                totalCost: 18,
                expectedMpg: 115m / 6.753m);

            // Add a partial fill
            await AddFuelAsync(
                mileage: 400,
                gallons: 3,
                totalCost: 7,
                expectedMpg: Constants.NO_MPG,
                isPartial: true);

            // Then add a full fill
            await AddFuelAsync(
                mileage: 500,
                gallons: 11,
                totalCost: 28,
                expectedMpg: 185m / 14m);

            // Make sure MPG also got applied to previous partial fill
            Assert.AreEqual(185m / 14m, fuelViewModel.FuelEntries[1].MPG);

            // Now add two partials
            await AddFuelAsync(
                mileage: 550,
                gallons: 1,
                totalCost: 3,
                expectedMpg: Constants.NO_MPG,
                isPartial: true);
            await AddFuelAsync(
                mileage: 750,
                gallons: 3,
                totalCost: 10,
                expectedMpg: Constants.NO_MPG,
                isPartial: true);

            // And then add a full
            await AddFuelAsync(
                mileage: 800,
                gallons: 8,
                totalCost: 26,
                expectedMpg: 300m / 12m);

            // Make sure MPG also got applied to previous partial fills
            Assert.AreEqual(300m / 12m, fuelViewModel.FuelEntries[1].MPG);
            Assert.AreEqual(300m / 12m, fuelViewModel.FuelEntries[2].MPG);


            // And now add a skipped fill, it shouldn't be able to calculate
            await AddFuelAsync(
                mileage: 1200,
                gallons: 7,
                totalCost: 40,
                expectedMpg: Constants.NO_MPG,
                skippedPrev: true);

            // And add a full fill, it should calculate
            await AddFuelAsync(
                mileage: 1300,
                gallons: 5,
                totalCost: 15,
                expectedMpg: 20);

            // And then add a partial fill
            await AddFuelAsync(
                mileage: 1400,
                gallons: 2,
                totalCost: 10,
                expectedMpg: Constants.NO_MPG,
                isPartial: true);

            // And a skipped fuel that's also partial
            await AddFuelAsync(
                mileage: 2000,
                gallons: 5,
                totalCost: 25,
                expectedMpg: Constants.NO_MPG,
                skippedPrev: true,
                isPartial: true);

            // And another partial
            await AddFuelAsync(
                mileage: 2100,
                gallons: 3,
                totalCost: 10,
                expectedMpg: Constants.NO_MPG,
                isPartial: true);

            // And add full - should NOT calculate since there's a skipped partial
            await AddFuelAsync(
                mileage: 2200,
                gallons: 5,
                totalCost: 26,
                expectedMpg: Constants.NO_MPG);
        }
    }
}
