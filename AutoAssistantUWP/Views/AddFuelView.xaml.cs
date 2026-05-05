using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddFuelView : PopupViewHostGeneric
    {
        public new AddFuelViewModel ViewModel
        {
            get { return base.ViewModel as AddFuelViewModel; }
            set { base.ViewModel = value; }
        }

        public AddFuelView()
        {
            this.InitializeComponent();

            base.IsEnabled = false;
        }

        public override void OnViewModelSetOverride()
        {
            switch (ViewModel.State)
            {
                case AddFuelViewModel.OperationState.Adding:
                    base.Title = "ADD FUEL";
                    break;

                case AddFuelViewModel.OperationState.Editing:
                    base.Title = "EDIT FUEL";
                    break;
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.IsEnabled = true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void tbOdometer_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.State == AddFuelViewModel.OperationState.Adding)
            {
                tbOdometer.Focus(FocusState.Programmatic);
            }
        }

        private void tbOdometer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbGallons.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void tbTotalCost_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.Save();
                e.Handled = true;
            }
        }

        private void tbGallons_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbTotalCost.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }
    }
}
