using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using Vx.Views;
using System.ComponentModel;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage
{
    public class AddVehicleViewModel : PopupComponentViewModel
    {
        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public ViewItemVehicle VehicleToEdit { get; private set; }

        private AddVehicleViewModel(MainScreenViewModel parent) : base(parent)
        {
            Title = State == OperationState.Editing ? "Edit Vehicle" : "Add Vehicle";
            AllowLightDismiss = false;
            PrimaryCommand = PopupCommand.Save(Save);
        }

        public static AddVehicleViewModel CreateForAdd(MainScreenViewModel parent)
        {
            return new AddVehicleViewModel(parent)
            {
                State = OperationState.Adding
            };
        }

        public static AddVehicleViewModel CreateForEdit(MainScreenViewModel parent, ViewItemVehicle vehicleToEdit)
        {
            var viewModel = new AddVehicleViewModel(parent)
            {
                State = OperationState.Editing,
                VehicleToEdit = vehicleToEdit,
                Nickname = vehicleToEdit.Nickname,
                Make = vehicleToEdit.Make,
                Model = vehicleToEdit.Model,
                Year = vehicleToEdit.Year,
                LicensePlate = vehicleToEdit.LicensePlate,
                VIN = vehicleToEdit.VIN,
                Notes = vehicleToEdit.Notes,
                InitialMileage = vehicleToEdit.InitialMileage,
                PurchasedFrom = vehicleToEdit.PurchasedFrom,
                AmountPurchasedFor = vehicleToEdit.AmountPurchasedFor
            };

            if (vehicleToEdit.DatePurchased != SqlDate.MinValue)
            {
                viewModel.DatePurchased = vehicleToEdit.DatePurchased;
            }

            viewModel.ListenToItem(vehicleToEdit.Identifier).Deleted += viewModel.Vehicle_Deleted;

            return viewModel;
        }

        #region EditableProperties

        private string _nickname = "";
        public string Nickname
        {
            get { return _nickname; }
            set { SetProperty(ref _nickname, value, nameof(Nickname)); }
        }

        private string _make = "";
        public string Make
        {
            get { return _make; }
            set { SetProperty(ref _make, value, nameof(Make)); }
        }

        private string _model = "";
        public string Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value, nameof(Model)); }
        }

        private string _year = "";
        public string Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value, nameof(Year)); }
        }

        private string _licensePlate = "";
        public string LicensePlate
        {
            get { return _licensePlate; }
            set { SetProperty(ref _licensePlate, value, nameof(LicensePlate)); }
        }

        private string _vin = "";
        public string VIN
        {
            get { return _vin; }
            set { SetProperty(ref _vin, value, nameof(VIN)); }
        }

        private string _notes = "";
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value, nameof(Notes)); }
        }

        private DateTime? _datePurchased;
        public DateTime? DatePurchased
        {
            get { return _datePurchased; }
            set { SetProperty(ref _datePurchased, value, nameof(DatePurchased)); }
        }

        private decimal _initialMileage = 0;
        public decimal InitialMileage
        {
            get { return _initialMileage; }
            set { SetProperty(ref _initialMileage, value, nameof(InitialMileage)); }
        }

        private string _purchasedFrom = "";
        public string PurchasedFrom
        {
            get { return _purchasedFrom; }
            set { SetProperty(ref _purchasedFrom, value, nameof(PurchasedFrom)); }
        }

        private string _amountPurchasedFor = "";
        public string AmountPurchasedFor
        {
            get { return _amountPurchasedFor; }
            set { SetProperty(ref _amountPurchasedFor, value, nameof(AmountPurchasedFor)); }
        }

        #endregion

        private void Vehicle_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public async void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Nickname))
                {
                    await new PortableMessageDialog("You must provide a nickname", "No nickname").ShowAsync();
                    return;
                }

                DataItemVehicle vehicle;

                if (VehicleToEdit != null)
                    vehicle = new DataItemVehicle()
                    {
                        Identifier = VehicleToEdit.Identifier
                    };

                else
                    vehicle = new DataItemVehicle() { Identifier = Guid.NewGuid() };

                vehicle.Nickname = Nickname.Trim();
                vehicle.Make = Make.Trim();
                vehicle.Model = Model.Trim();
                vehicle.Year = Year.Trim();
                vehicle.LicensePlate = LicensePlate.Trim();
                vehicle.VIN = VIN.Trim();
                vehicle.Notes = Notes.Trim();
                vehicle.DatePurchased = DatePurchased.GetValueOrDefault(SqlDate.MinValue);
                if (InitialMileage < 0)
                    vehicle.InitialMileage = 0;
                else
                    vehicle.InitialMileage = InitialMileage;
                vehicle.PurchasedFrom = PurchasedFrom.Trim();
                vehicle.AmountPurchasedFor = AmountPurchasedFor.Trim();

                DataChanges changes = new DataChanges();
                changes.Vehicles.Add(vehicle);

                await AutoAssistantApp.Current.SaveChanges(changes);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            base.GoBack();
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = "Nickname",
                    PlaceholderText = "ex: Hitch",
                    Text = VxValue.Create(Nickname, v => Nickname = v)
                },

                new AdaptiveGridPanelComponent
                {
                    ColumnSpacing = 12,
                    MinColumnWidth = 120,
                    Children =
                    {
                        new TextBox
                        {
                            Header = "Year",
                            Text = VxValue.Create(Year, v => Year = v),
                            InputScope = InputScope.Number,
                            PlaceholderText = "ex: 2020",
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new TextBox
                        {
                            Header = "Make",
                            Text = VxValue.Create(Make, v => Make = v),
                            PlaceholderText = "ex: Toyota",
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new TextBox
                        {
                            Header = "Model",
                            Text = VxValue.Create(Model, v => Model = v),
                            PlaceholderText = "ex: Tacoma",
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new NumberTextBox
                        {
                            Header = "Initial mileage",
                            Number = VxValue.Create<double?>(InitialMileage == 0 ? null : (double)InitialMileage, v => InitialMileage = v == null ? 0 : (decimal)v),
                            PlaceholderText = "ex: 32,602",
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new TextBox
                        {
                            Header = "Amount purchased for",
                            Text = VxValue.Create(AmountPurchasedFor, v => AmountPurchasedFor = v),
                            PlaceholderText = "ex: 6400",
                            InputScope = InputScope.Number,
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new TextBox
                        {
                            Header = "Purchased from",
                            Text = VxValue.Create(PurchasedFrom, v => PurchasedFrom = v),
                            PlaceholderText = "ex: Michaels Toyota",
                            Margin = new Thickness(0, 12, 0, 0)
                        }
                    }
                },

                new DatePicker
                {
                    Header = "Date purchased",
                    Value = VxValue.Create(DatePurchased, v => DatePurchased = v),
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new TextBox
                {
                    Header = "License plate",
                    Text = VxValue.Create(LicensePlate, v => LicensePlate = v),
                    PlaceholderText = "ex: AUD-5702",
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new TextBox
                {
                    Header = "VIN (Vehicle Identification Number)",
                    Text = VxValue.Create(VIN, v => VIN = v),
                    PlaceholderText = "ex: 1FTEX27L5WNA08863",
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new MultilineTextBox
                {
                    Header = "Notes",
                    Text = VxValue.Create(Notes, v => Notes = v),
                    Height = 150,
                    Margin = new Thickness(0, 12, 0, 0)
                }
            );
        }
    }
}
