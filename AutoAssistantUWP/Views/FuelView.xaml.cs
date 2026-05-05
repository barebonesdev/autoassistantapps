using AutoAssistantAppDataLibrary.ViewItems;
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
    public sealed partial class FuelView : MainScreenContentViewHostGeneric
    {
        public FuelView()
        {
            this.InitializeComponent();
        }

        public new FuelViewModel ViewModel
        {
            get { return base.ViewModel as FuelViewModel; }
            set { base.ViewModel = value; }
        }

        private void ButtonAddFuel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddFuel();
        }

        private void ListViewFuelEntries_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewFuelEntry(e.ClickedItem as ViewItemFuelEntry);
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 568)
            {
                VisualStateManager.GoToState(this, e.NewSize.Width < 640 ? "FullSizeSmaller" : "FullSize", true);

                if (PivotFuels.Content != null)
                {
                    ScrollViewerPivotOverview.Content = null;
                    StackPanelFirstColumn.Children.Insert(0, StackPanelOverview);

                    PivotFuels.Content = null;
                    GridFullSizeFuels.Children.Add(ListViewFuelEntries);

                    ScrollViewerPivotEstimator.Content = null;
                    StackPanelFullSizeEstimatorContainer.Children.Add(StackPanelTripCostEstimatorContent);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Compact", true);

                if (PivotFuels.Content == null)
                {
                    StackPanelFirstColumn.Children.Remove(StackPanelOverview);
                    ScrollViewerPivotOverview.Content = StackPanelOverview;

                    GridFullSizeFuels.Children.Remove(ListViewFuelEntries);
                    PivotFuels.Content = ListViewFuelEntries;

                    StackPanelFullSizeEstimatorContainer.Children.Remove(StackPanelTripCostEstimatorContent);
                    ScrollViewerPivotEstimator.Content = StackPanelTripCostEstimatorContent;
                }
            }
        }

        private void PivotCompact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotCompact.SelectedItem == PivotCostEstimator)
            {
                CommandBarForPivot.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandBarForPivot.Visibility = Visibility.Visible;
            }
        }

        private void ButtonImportFuel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportFuel();
        }

        private void ButtonExportFuel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExportFuel();
        }
    }
}
