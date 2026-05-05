using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AutoAssistantUWP.Views
{
    public sealed partial class ExportFuelToCsvView : PopupViewHostGeneric
    {
        public new ExportFuelToCsvViewModel ViewModel
        {
            get { return base.ViewModel as ExportFuelToCsvViewModel; }
            set { base.ViewModel = value; }
        }

        public ExportFuelToCsvView()
        {
            this.InitializeComponent();
        }

        private async void ButtonSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            ButtonSaveToFile.IsEnabled = false;
            try
            {
                FileSavePicker savePicker = new FileSavePicker()
                {
                    FileTypeChoices =
                    {
                        { "CSV", new List<string>() { ".csv" } }
                    },
                    SuggestedFileName = ViewModel.MainScreenViewModel.CurrentVehicle?.Nickname + "FuelRecords",
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                var storageFile = await savePicker.PickSaveFileAsync();
                await FileIO.WriteTextAsync(storageFile, ViewModel.CsvText);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
            finally
            {
                ButtonSaveToFile.IsEnabled = true;
                ViewModel.GoBack();
            }
        }
    }
}
