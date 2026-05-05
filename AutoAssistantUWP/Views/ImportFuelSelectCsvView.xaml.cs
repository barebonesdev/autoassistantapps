using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
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
    public sealed partial class ImportFuelSelectCsvView : PopupViewHostGeneric
    {
        public new ImportFuelSelectCsvViewModel ViewModel
        {
            get { return base.ViewModel as ImportFuelSelectCsvViewModel; }
            set { base.ViewModel = value; }
        }

        public ImportFuelSelectCsvView()
        {
            this.InitializeComponent();
        }

        private async void ButtonImportFromFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".csv");

                var file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            ViewModel.CsvText = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch { }
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Continue();
        }
    }
}
