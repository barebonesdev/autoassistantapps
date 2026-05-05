using AutoAssistantAppDataLibrary;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using AutoAssistantAppDataLibrary.Windows;
using AutoAssistantAppDataLibraryTests.Helpers;
using BareMvvm.Core.App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibraryTests.FakeData
{
    public class AutoAssistantTestApp : AutoAssistantApp
    {
        public static new AutoAssistantTestApp Current
        {
            get { return PortableApp.Current as AutoAssistantTestApp; }
        }

        public MainAppWindow MainAppWindow { get; private set; }

        /// <summary>
        /// Hook up the dispatcher and initialize the app
        /// </summary>
        /// <returns></returns>
        public static Task InitializeApp()
        {
            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new FakeDispatcher(); };

            // Initialize the app
            return PortableApp.InitializeAsync((PortableApp)Activator.CreateInstance(typeof(AutoAssistantTestApp)));
        }

        public async Task LaunchMainAsync()
        {
            if (MainAppWindow == null)
            {
                MainAppWindow = new MainAppWindow();
                await RegisterWindowAsync(MainAppWindow, new FakeNativeWindow());
            }

            await MainAppWindow.GetViewModel().HandleNormalLaunchActivation();
        }

        public MainWindowViewModel MainWindowViewModel
        {
            get { return MainAppWindow?.GetViewModel(); }
        }

        public MainScreenViewModel MainScreenViewModel
        {
            get { return MainWindowViewModel?.Content as MainScreenViewModel; }
        }

        public static async Task InitializeWithBlankVehicleAsync()
        {
            await InitializeApp();
            await Current.InitializeWithBlankVehicleHelperAsync();
        }

        private async Task InitializeWithBlankVehicleHelperAsync()
        {
            // Delete any existing accounts
            var allAccounts = await AccountsManager.GetAllAccounts();
            foreach (var acc in allAccounts)
            {
                await AccountsManager.Delete(acc.LocalAccountId);
            }

            await LaunchMainAsync();

            var welcomeViewModel = MainWindowViewModel.Content as WelcomeViewModel;
            welcomeViewModel.CreateAccount();

            var createAccountViewModel = MainWindowViewModel.Popups.First() as CreateAccountViewModel;
            createAccountViewModel.Username = "test";
            createAccountViewModel.Password = "password";
            createAccountViewModel.ConfirmPassword = "password";

            await createAccountViewModel.CreateLocalAccountAsync();

            var garageViewModel = MainScreenViewModel.Content as GarageViewModel;
            await garageViewModel.LoadAsync();

            garageViewModel.AddVehicle();

            var addVehicleViewModel = MainScreenViewModel.Popups.First() as AddVehicleViewModel;
            addVehicleViewModel.Nickname = "Manny";
            addVehicleViewModel.Year = "2000";
            addVehicleViewModel.Make = "Toyota";
            addVehicleViewModel.Model = "4Runner";

            addVehicleViewModel.Save();

            await ChangeHelper.WaitTillCountAsync(garageViewModel.Vehicles, 1);

            await garageViewModel.OpenVehicle(garageViewModel.Vehicles.First());
        }
    }
}
