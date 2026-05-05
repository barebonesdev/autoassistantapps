using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using CsvHelper;
using System.IO;
using ToolsPortable;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.ViewItemsGroup;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelPreviewImportViewModel : BaseMainScreenViewModelChild
    {
        public ImportFuelPreviewImportViewModel(BaseViewModel parent, string csv) : base(parent)
        {
            ParseCsv(csv);
        }

        public List<DataItemFuelEntry> Entries { get; private set; } = new List<DataItemFuelEntry>();

        public void AddToExistingRecords()
        {
            AddRecords(new DataChanges());
        }

        public async void ReplaceExistingRecords()
        {
            try
            {
                DataChanges changes = new DataChanges();

                VehicleViewItemsGroup vehicleItems = await VehicleViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentVehicle);
                foreach (var f in vehicleItems.Fuel)
                {
                    changes.Fuel.DeleteItem(f.Identifier);
                }

                AddRecords(changes);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Failed to save. Your error has been reported.", "Error saving").ShowAsync();
                return;
            }
        }

        private async void AddRecords(DataChanges changes)
        {
            try
            {
                Guid vehicleId = MainScreenViewModel.CurrentVehicleId;
                if (vehicleId == Guid.Empty)
                {
                    throw new Exception("VehicleId was empty");
                }

                foreach (var f in Entries)
                {
                    f.Identifier = Guid.NewGuid();
                    f.VehicleIdentifier = MainScreenViewModel.CurrentVehicleId;
                    f.Date = DateTime.SpecifyKind(f.Date, DateTimeKind.Utc);

                    changes.Fuel.Add(f);
                }

                await AutoAssistantApp.Current.SaveChanges(changes);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Failed to save. Your error has been reported.", "Error saving").ShowAsync();
                return;
            }

            base.MainScreenViewModel.Popups.Clear();
        }

        private void ParseCsv(string csv)
        {
            try
            {
                Entries = new List<DataItemFuelEntry>();

                using (StringReader stringReader = new StringReader(csv))
                {
                    using (CsvReader reader = new CsvReader(stringReader))
                    {
                        while (reader.Read())
                        {
                            DataItemFuelEntry entry;

                            try
                            {
                                entry = ParseRecord(reader.CurrentRecord);
                            }

                            catch (ParseException p)
                            {
                                new PortableMessageDialog(p.Message + "\n\nRow: " + reader.Row, "Error parsing CSV").ShowAsync();
                                return;
                            }

                            Entries.Add(entry);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                new PortableMessageDialog(e.ToString(), "Error parsing CSV").ShowAsync();
            }
        }

        private DataItemFuelEntry ParseRecord(string[] record)
        {
            if (record.Length < 9)
                throw new ParseException("Row did not have the necessary number of columns, only had " + record.Length + " columns.");

            return new DataItemFuelEntry()
            {
                Mileage = parseMileage(record[0]),
                CostPerGallon = parseCostPerGallon(record[1]),
                Gallons = parseGallons(record[2]),
                Date = parseDate(record[3]),
                StoreName = record[4],
                FuelType = parseFuelType(record[5]),
                PartialFill = parsePartialFillup(record[6]),
                SkippedEnteringPreviousFillup = parseSkippedPrevious(record[7]),
                Notes = record[8]
            };
        }

        private class ParseException : Exception
        {
            public ParseException(string message) : base(message)
            {

            }
        }

        private decimal parseMileage(string value)
        {
            try
            {
                return decimal.Parse(value);
            }

            catch
            {
                throw new ParseException("Poorly formatted odometer: " + value);
            }
        }

        private decimal parseCostPerGallon(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Constants.NO_COST;

            try
            {
                return decimal.Parse(value);
            }

            catch
            {
                throw new ParseException("Poorly formatted cost per gallon: " + value);
            }
        }

        private decimal parseGallons(string value)
        {
            try
            {
                return decimal.Parse(value);
            }

            catch
            {
                throw new ParseException("Poorly formatted gallons: " + value);
            }
        }

        private DateTime parseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.Today;

            try
            {
                return DateTime.Parse(value);
            }

            catch
            {
                throw new ParseException("Poorly formatted date: " + value);
            }
        }

        private AutoAssistantLibrary.Items.SyncItemFuelEntry.FuelTypes parseFuelType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return AutoAssistantLibrary.Items.SyncItemFuelEntry.FuelTypes.None;

            try
            {
                return (AutoAssistantLibrary.Items.SyncItemFuelEntry.FuelTypes)Enum.Parse(typeof(AutoAssistantLibrary.Items.SyncItemFuelEntry.FuelTypes), value);
            }

            catch
            {
                throw new ParseException("Poorly formatted fuel type: " + value);
            }
        }

        private bool parsePartialFillup(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                return bool.Parse(value);
            }

            catch
            {
                throw new ParseException("Poorly formatted partial fillup: " + value);
            }
        }

        private bool parseSkippedPrevious(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                return bool.Parse(value);
            }

            catch { throw new ParseException("Poorly formmatted skipped previous: " + value); }
        }
    }
}
