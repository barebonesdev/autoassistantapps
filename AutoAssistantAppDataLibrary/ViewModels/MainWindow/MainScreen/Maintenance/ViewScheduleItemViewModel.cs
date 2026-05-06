using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using ToolsPortable;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class ViewScheduleItemViewModel : PopupComponentViewModel
    {
        public ViewItemMaintenanceScheduleItem ScheduleItem { get; private set; }

        public MyObservableList<ViewItemMaintenanceRecordEntry> Records { get; private set; }

        private VehicleViewItemsGroup _vehicleViewItemsGroup;

        public ViewScheduleItemViewModel(MainScreenViewModel parent, ViewItemMaintenanceScheduleItem scheduleItem) : base(parent)
        {
            Title = "View schedule item";
            ScheduleItem = scheduleItem;
            Commands = new PopupCommand[]
            {
                PopupCommand.Edit(Edit),
                PopupCommand.DeleteWithQuickConfirm(Delete)
            };

            this.ListenToItem(scheduleItem.Identifier).Deleted += ViewScheduleItemViewModel_Deleted;

            LoadRecords();
        }

        private async void LoadRecords()
        {
            try
            {
                _vehicleViewItemsGroup = await FindAncestor<MainScreenViewModel>().CurrentVehicle.GetViewItemsGroupAsync();
                Records = _vehicleViewItemsGroup.MaintenanceRecords.Sublist(record => record.ServicesPerformed.Any(service => service.Identifier == ScheduleItem.Identifier));
                OnPropertyChanged(nameof(Records));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewScheduleItemViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            ShowPopup(AddScheduleItemViewModel.CreateForEdit(FindAncestor<MainScreenViewModel>(), ScheduleItem));
        }

        public async void Delete()
        {
            try
            {
                await FindAncestor<MainScreenViewModel>().DeleteMaintenanceSchedule(ScheduleItem.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewRecord(ViewItemMaintenanceRecordEntry record)
        {
            ShowPopup(new ViewMaintenanceRecordViewModel(FindAncestor<MainScreenViewModel>(), record));
        }

        protected override View Render()
        {
            List<View> views = new List<View>()
            {
                new TextBlock
                {
                    Text = ScheduleItem.Title,
                    IsTextSelectionEnabled = true
                }.TitleStyle(),

                new TextBlock
                {
                    Text = ScheduleItem.Subtitle,
                    FontWeight = FontWeights.SemiBold,
                    TextColor = Theme.Current.AccentColor,
                    IsTextSelectionEnabled = true
                },

                new TextBlock
                {
                    Text = ScheduleItem.Details,
                    Margin = new Thickness(0, 12, 0, 0),
                    IsTextSelectionEnabled = true
                },

                new TextBlock
                {
                    Text = "Records",
                    Margin = new Thickness(0, 12, 0, 0)
                }.TitleStyle()
            };

            if (Records.Count == 0)
            {
                views.Add(new TextBlock
                {
                    Text = "No records yet",
                    TextColor = Theme.Current.SubtleForegroundColor
                });

            }
            else
            {
                foreach (var record in Records)
                {
                    views.Add(new LinearLayout
                    {
                        Margin = new Thickness(0, 6, 0, 0),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = record.Title,
                                FontSize = Theme.Current.SubtitleFontSize,
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = record.Subtitle,
                                TextColor = Theme.Current.AccentColor,
                                WrapText = false
                            },

                            string.IsNullOrWhiteSpace(record.Details) ? null : new TextBlock
                            {
                                Text = record.Details,
                                TextColor = Theme.Current.SubtleForegroundColor,
                                MaxLines = 1,
                                WrapText = false
                            }
                        },
                        Tapped = () => ViewRecord(record),
                    });
                }
            }

            return RenderGenericPopupContent(views);
        }
    }
}
