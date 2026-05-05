using AutoAssistantAppDataLibrary;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoAssistantUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportFuelPreviewImportView : PopupViewHostGeneric
    {
        public new ImportFuelPreviewImportViewModel ViewModel
        {
            get { return base.ViewModel as ImportFuelPreviewImportViewModel; }
            set { base.ViewModel = value; }
        }

        public ImportFuelPreviewImportView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddRow(new string[]
            {
                "#", "Odometer", "Cost per gallon", "Gallons", "Date", "Store name", "Fuel type", "Partial fillup", "Skipped previous", "Notes"
            });

            int i = 1;
            foreach (var entry in ViewModel.Entries)
            {
                AddRow(new string[]
                {
                    i.ToString(),
                    entry.Mileage.ToString(),
                    entry.CostPerGallon == Constants.NO_COST ? "" : entry.CostPerGallon.ToString(),
                    entry.Gallons.ToString(),
                    entry.Date.ToString(),
                    entry.StoreName,
                    entry.FuelType.ToString(),
                    entry.PartialFill.ToString(),
                    entry.SkippedEnteringPreviousFillup.ToString(),
                    entry.Notes
                });

                i++;
            }
        }

        private void ButtonAddToExisting_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddToExistingRecords();
        }

        private void ButtonReplaceExisting_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ReplaceExistingRecords();
        }

        private void AddRow(string[] values)
        {
            GridTable.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int row = GridTable.RowDefinitions.Count - 1;


            for (int i = 0; i < values.Length; i++)
                AddCell(row, i, values[i]);
        }

        private void AddCell(int row, int col, string value)
        {
            if (col >= GridTable.ColumnDefinitions.Count)
            {
                GridTable.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            }

            Border cell = new Border()
            {
                Padding = new Thickness(6, 4, 6, 4),
                BorderBrush = Application.Current.Resources["SystemControlHighlightAccentBrush"] as Brush,
                BorderThickness = new Thickness(0, row == 0 ? 0.5 : 0, 0.5, 0.5),
                Child = new TextBlock()
                {
                    Text = value
                }
            };

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);

            GridTable.Children.Add(cell);
        }
    }
}
