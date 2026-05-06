using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Extensions;
using System.Runtime.CompilerServices;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class ViewMaintenanceRecordViewModel : PopupComponentViewModel
    {
        public ViewItemMaintenanceRecordEntry RecordEntry { get; private set; }

        public string DoneByString
        {
            get
            {
                return GetBindedValue(nameof(RecordEntry.DoneBy), delegate
                {
                    if (string.IsNullOrWhiteSpace(RecordEntry.DoneBy))
                    {
                        return null;
                    }

                    return "Done by: " + RecordEntry.DoneBy.Trim();
                });
            }
        }

        public ViewMaintenanceRecordViewModel(MainScreenViewModel parent, ViewItemMaintenanceRecordEntry recordEntry) : base(parent)
        {
            Title = "View maintenance record";
            RecordEntry = recordEntry;
            Commands = new PopupCommand[]
            {
                PopupCommand.Edit(Edit),
                PopupCommand.DeleteWithQuickConfirm(Delete)
            };

            this.ListenToItem(recordEntry.Identifier).Deleted += ViewMaintenanceRecordViewModel_Deleted;
        }

        private string GetBindedValue(string recordEntryPropertyName, Func<string> convert, [CallerMemberName]string propertyName = null)
        {
            return GetBindedValue(RecordEntry, recordEntryPropertyName, convert, propertyName);
        }

        private void ViewMaintenanceRecordViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            ShowPopup(AddMaintenanceRecordViewModel.CreateForEdit(FindAncestor<MainScreenViewModel>(), RecordEntry));
        }

        public async void Delete()
        {
            try
            {
                await FindAncestor<MainScreenViewModel>().DeleteMaintenanceRecord(RecordEntry.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewScheduleItem(ViewItemMaintenanceScheduleItem item)
        {
            ShowPopup(new ViewScheduleItemViewModel(FindAncestor<MainScreenViewModel>(), item));
        }

        protected override View Render()
        {
            List<View> views = new List<View>()
            {
                new TextBlock
                {
                    Text = RecordEntry.Title,
                    IsTextSelectionEnabled = true
                }.TitleStyle(),

                new TextBlock
                {
                    Text = RecordEntry.Subtitle,
                    FontWeight = FontWeights.SemiBold,
                    TextColor = Theme.Current.AccentColor,
                    IsTextSelectionEnabled = true
                },

                string.IsNullOrWhiteSpace(DoneByString) ? null : new TextBlock
                {
                    Text = DoneByString,
                    WrapText = false
                }
            };

            foreach (var service in RecordEntry.ServicesPerformed)
            {
                views.Add(new Border
                {
                    CornerRadius = 4,
                    BackgroundColor = Theme.Current.AccentColor,
                    Tapped = () => ViewScheduleItem(service),
                    Margin = new Thickness(0, 4, 0, 0),
                    Content = new TextBlock
                    {
                        Text = "✅ " + service.Title,
                        Margin = new Thickness(4),
                        TextColor = System.Drawing.Color.White,
                        WrapText = false
                    },
                    HorizontalAlignment = HorizontalAlignment.Left
                });
            }

            views.Add(new TextBlock
            {
                Text = RecordEntry.Details,
                Margin = new Thickness(0, 12, 0, 0),
                IsTextSelectionEnabled = true
            });

            return RenderGenericPopupContent(views);
        }
    }
}
