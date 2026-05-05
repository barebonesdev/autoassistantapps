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
    public sealed partial class AddScheduleItemView : PopupViewHostGeneric
    {
        public new AddScheduleItemViewModel ViewModel
        {
            get { return base.ViewModel as AddScheduleItemViewModel; }
            set { base.ViewModel = value; }
        }

        public AddScheduleItemView()
        {
            this.InitializeComponent();

            InterfacesUWP.MyScrollViewerExtensions.EnsureExpandableTextBoxRemainsVisibleWhileTyping(ScrollViewerContent, tbDetails);
        }

        public override void OnViewModelSetOverride()
        {
            switch (ViewModel.State)
            {
                case AddScheduleItemViewModel.OperationState.Adding:
                    base.Title = "ADD SCHEDULE ITEM";
                    break;

                case AddScheduleItemViewModel.OperationState.Editing:
                    base.Title = "EDIT SCHEDULE ITEM";
                    break;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void tbTitle_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.State == AddScheduleItemViewModel.OperationState.Adding)
            {
                tbTitle.Focus(FocusState.Programmatic);
            }
        }

        private void tbTitle_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbMileageInterval.Focus(FocusState.Programmatic);
            }
        }

        private void tbMileageInterval_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbMonthInterval.Focus(FocusState.Programmatic);
            }
        }

        private void tbMonthInterval_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbEstimatedCost.Focus(FocusState.Programmatic);
            }
        }

        private void tbEstimatedCost_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbDetails.Focus(FocusState.Programmatic);
            }
        }
    }
}
