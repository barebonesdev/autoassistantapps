using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.Extensions;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class AddScheduleItemViewModel : PopupComponentViewModel
    {
        public AccountDataItem Account { get; private set; }
        public ViewItemVehicle Vehicle { get; private set; }

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public ViewItemMaintenanceScheduleItem ItemToEdit { get; private set; }

        private string _title = "";
        public string ScheduleTitle
        {
            get { return _title; }
            set { SetProperty(ref _title, value, nameof(ScheduleTitle)); }
        }

        private string _details = "";
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        private decimal _mileageInterval = Constants.NO_MILES;
        public decimal MileageInterval
        {
            get { return _mileageInterval; }
            set { SetProperty(ref _mileageInterval, value, nameof(MileageInterval)); }
        }

        private short _timeInterval = Constants.NO_MONTHS;
        public short MonthInterval
        {
            get { return _timeInterval; }
            set { SetProperty(ref _timeInterval, value, nameof(MonthInterval)); }
        }

        private decimal _estimatedCost = Constants.NO_COST;
        public decimal EstimatedCost
        {
            get { return _estimatedCost; }
            set { SetProperty(ref _estimatedCost, value, nameof(EstimatedCost)); }
        }

        private AddScheduleItemViewModel(MainScreenViewModel parent) : base(parent)
        {
            AllowLightDismiss = false;
            Vehicle = parent.CurrentVehicle;
            if (Vehicle == null)
            {
                throw new NullReferenceException("CurrentVehicle was null");
            }
            PrimaryCommand = PopupCommand.Save(Save);
        }

        public static AddScheduleItemViewModel CreateForAdd(MainScreenViewModel parent)
        {
            return new AddScheduleItemViewModel(parent)
            {
                State = OperationState.Adding,
                Title = "Add schedule"
            };
        }

        public static AddScheduleItemViewModel CreateForEdit(MainScreenViewModel parent, ViewItemMaintenanceScheduleItem item)
        {
            return new AddScheduleItemViewModel(parent)
            {
                State = OperationState.Editing,
                ItemToEdit = item,
                ScheduleTitle = item.Title,
                Details = item.Details,
                MileageInterval = item.MileageInterval,
                MonthInterval = item.MonthInterval,
                EstimatedCost = item.EstimatedCost,
                Title = "Edit schedule"
            };
        }

        public async void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ScheduleTitle))
                {
                    await new PortableMessageDialog("You must enter a title.", "No title").ShowAsync();
                    return;
                }

                if (MileageInterval == Constants.NO_MILES && MonthInterval == Constants.NO_MONTHS)
                {
                    await new PortableMessageDialog("You must either assign a mileage interval or a month interval.", "No interval").ShowAsync();
                    return;
                }

                if (MileageInterval != Constants.NO_MILES && MileageInterval <= 0)
                {
                    await new PortableMessageDialog("Mileage interval must either be a positive number or left empty.", "Invalid mileage interval").ShowAsync();
                    return;
                }

                if (MonthInterval != Constants.NO_MONTHS && MonthInterval <= 0)
                {
                    await new PortableMessageDialog("Month interval must either be a positive number or left empty.", "Invalid month interval").ShowAsync();
                    return;
                }

                DataItemMaintenanceScheduleItem dataItem;

                if (ItemToEdit != null)
                    dataItem = new DataItemMaintenanceScheduleItem()
                    {
                        Identifier = ItemToEdit.Identifier
                    };

                else
                    dataItem = new DataItemMaintenanceScheduleItem()
                    {
                        Identifier = Guid.NewGuid(),
                        VehicleIdentifier = Vehicle.Identifier
                    };

                dataItem.Title = ScheduleTitle.Trim();
                dataItem.Details = Details.Trim();
                dataItem.MileageInterval = MileageInterval;
                dataItem.MonthInterval = MonthInterval;
                dataItem.EstimatedCost = EstimatedCost;

                DataChanges changes = new DataChanges();
                changes.MaintenanceSchedule.Add(dataItem);

                await AutoAssistantApp.Current.SaveChanges(changes);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Failed to save. Your error has been reported.", "Error saving").ShowAsync();
                return;
            }

            base.GoBack();
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = "Title",
                    PlaceholderText = "Rotate tires",
                    Text = VxValue.Create(ScheduleTitle, v => ScheduleTitle = v),
                    AutoFocus = true,
                    AutoMoveToNextTextBox = true
                },

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new NumberTextBox
                        {
                            Header = "Mileage interval",
                            PlaceholderText = "5000",
                            Number = VxValue.Create<double?>(MileageInterval == Constants.NO_MILES ? null : (double)MileageInterval, v => MileageInterval = v != null ? (decimal)v : Constants.NO_MILES),
                            Margin = new Thickness(0, 12, 6, 0)
                        }.LinearLayoutWeight(1),

                        new NumberTextBox
                        {
                            Header = "Month interval",
                            PlaceholderText = "4",
                            Number = VxValue.Create<double?>(MonthInterval == Constants.NO_MONTHS ? null : (double)MonthInterval, v => MonthInterval = v != null ? (short)v : Constants.NO_MONTHS),
                            Margin = new Thickness(6, 12, 0, 0),
                            OnSubmit = Save
                        }.LinearLayoutWeight(1)
                    }
                },

                new NumberTextBox
                {
                    Header = "Estimated cost",
                    PlaceholderText = "49.95",
                    Number = VxValue.Create<double?>(EstimatedCost == Constants.NO_COST ? null : (double)EstimatedCost, v => EstimatedCost = v != null ? (decimal)v : Constants.NO_COST),
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new MultilineTextBox
                {
                    Header = "Details",
                    Height = 150,
                    Text = VxValue.Create(Details, v => Details = v),
                    Margin = new Thickness(0, 12, 0, 0)
                }

            );
        }
    }
}
