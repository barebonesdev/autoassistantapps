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
    public sealed partial class AddMaintenanceRecordView : PopupViewHostGeneric
    {
        public new AddMaintenanceRecordViewModel ViewModel
        {
            get { return base.ViewModel as AddMaintenanceRecordViewModel; }
            set { base.ViewModel = value; }
        }

        public AddMaintenanceRecordView()
        {
            this.InitializeComponent();
            InterfacesUWP.MyScrollViewerExtensions.EnsureExpandableTextBoxRemainsVisibleWhileTyping(ScrollViewerContent, tbDetails);
        }

        public override void OnViewModelSetOverride()
        {
            switch (ViewModel.State)
            {
                case AddMaintenanceRecordViewModel.OperationState.Adding:
                    base.Title = "ADD MAINTENANCE RECORD";
                    break;

                case AddMaintenanceRecordViewModel.OperationState.Editing:
                    base.Title = "EDIT MAINTENANCE RECORD";
                    break;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void tbTitle_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.State == AddMaintenanceRecordViewModel.OperationState.Adding)
            {
                tbTitle.Focus(FocusState.Programmatic);
            }
        }

        private void tbTitle_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbMileage.Focus(FocusState.Programmatic);
            }
        }

        private void tbEstimatedCost_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbDetails.Focus(FocusState.Programmatic);
            }
        }

        private void tbCost_KeyUp(object sender, KeyRoutedEventArgs e)
        {

        }

        private void tbMileage_KeyUp(object sender, KeyRoutedEventArgs e)
        {

        }

        private void ListBoxSelectedServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListBoxSelectedServices.SelectedItem as ViewItemMaintenanceScheduleItem;
            if (item != null)
            {
                ViewModel.UnselectService(item);
            }
        }

        private void ListBoxUnselectedServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListBoxUnselectedServices.SelectedItem as ViewItemMaintenanceScheduleItem;
            if (item != null)
            {
                ViewModel.SelectService(item);
            }
        }
    }
}
