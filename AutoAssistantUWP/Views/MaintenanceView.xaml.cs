using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MaintenanceView : MainScreenContentViewHostGeneric
    {
        public new MaintenanceViewModel ViewModel
        {
            get { return base.ViewModel as MaintenanceViewModel; }
            set { base.ViewModel = value; }
        }

        public MaintenanceView()
        {
            this.InitializeComponent();
        }

        private void ListViewSchedule_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewScheduleItem(e.ClickedItem as ViewItemMaintenanceScheduleItem);
        }

        private void ButtonAddScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddScheduleItem();
        }

        private void ListViewRecords_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewMaintenanceRecord(e.ClickedItem as ViewItemMaintenanceRecordEntry);
        }

        private void ButtonAddRecord_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMaintenanceRecord();
        }

        private void ListViewUpcomingServices_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewUpcomingService(e.ClickedItem as ViewItemUpcomingMaintenanceScheduleItem);
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 550)
            {
                VisualStateManager.GoToState(this, "FullSize", true);

                if (PivotItemUpcoming.Content != null)
                {
                    PivotItemUpcoming.Content = null;
                    GridFullSize.Children.Add(ListViewUpcomingServices);

                    PivotItemRecords.Content = null;
                    GridFullSizeRecords.Children.Add(ListViewRecords);

                    PivotItemSchedule.Content = null;
                    GridFullSizeSchedule.Children.Add(ListViewSchedule);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Compact", true);

                if (PivotItemUpcoming.Content == null)
                {
                    GridFullSize.Children.Remove(ListViewUpcomingServices);
                    PivotItemUpcoming.Content = ListViewUpcomingServices;

                    GridFullSizeRecords.Children.Remove(ListViewRecords);
                    PivotItemRecords.Content = ListViewRecords;

                    GridFullSizeSchedule.Children.Remove(ListViewSchedule);
                    PivotItemSchedule.Content = ListViewSchedule;
                }
            }
        }

        private void AppBarButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (PivotCompact.SelectedItem == PivotItemSchedule)
            {
                ViewModel.AddScheduleItem();
            }
            else
            {
                ViewModel.AddMaintenanceRecord();
            }
        }

        private void PivotCompact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotCompact.SelectedItem == PivotItemSchedule)
            {
                AppBarButtonAdd.Label = "Add schedule item";
            }
            else
            {
                AppBarButtonAdd.Label = "Add record";
            }

            AppBarButtonSearch.Visibility = PivotCompact.SelectedItem == PivotItemRecords ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonSearchRecords_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchRecords();
        }

        private void AppBarButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchRecords();
        }
    }
}
