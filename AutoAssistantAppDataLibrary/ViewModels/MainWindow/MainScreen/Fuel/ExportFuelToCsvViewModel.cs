using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using BareMvvm.Core.ViewModels;
using CsvHelper;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ExportFuelToCsvViewModel : BaseMainScreenViewModelChild
    {
        public ExportFuelToCsvViewModel(BaseViewModel parent) : base(parent)
        {
        }

        private string m_csvText;
        public string CsvText
        {
            get { return m_csvText; }
            set { SetProperty(ref m_csvText, value, nameof(CsvText)); }
        }

        protected override async Task LoadAsyncOverride()
        {
            VehicleViewItemsGroup vehicleItems = await VehicleViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentVehicle);

            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                using (CsvWriter writer = new CsvWriter(stringWriter))
                {
                    WriteHeader(writer);

                    foreach (var fuelEntry in vehicleItems.Fuel)
                    {
                        writer.NextRecord();

                        WriteRecord(writer, fuelEntry);
                    }
                }
            }

            CsvText = sb.ToString();

            await base.LoadAsyncOverride();
        }

        private void WriteHeader(CsvWriter writer)
        {
            writer.WriteField("Odometer");
            writer.WriteField("Cost per gallon");
            writer.WriteField("Gallons");
            writer.WriteField("Date");
            writer.WriteField("Store name");
            writer.WriteField("Fuel type");
            writer.WriteField("Partial fillup");
            writer.WriteField("Skipped previous");
            writer.WriteField("Notes");
        }

        private void WriteRecord(CsvWriter writer, ViewItemFuelEntry entry)
        {
            writer.WriteField(entry.Mileage);
            writer.WriteField(entry.CostPerGallon);
            writer.WriteField(entry.Gallons);
            writer.WriteField(entry.Date);
            writer.WriteField(entry.StoreName);
            writer.WriteField(entry.FuelType);
            writer.WriteField(entry.PartialFill);
            writer.WriteField(entry.SkippedEnteringPreviousFillup);
            writer.WriteField(entry.Notes);
        }
    }
}
