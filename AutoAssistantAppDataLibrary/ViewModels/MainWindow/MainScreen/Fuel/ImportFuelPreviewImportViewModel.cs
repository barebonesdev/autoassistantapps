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
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelPreviewImportViewModel : PopupComponentViewModel
    {
        public ImportFuelPreviewImportViewModel(BaseViewModel parent, string csv) : base(parent)
        {
            Title = "Import fuel records";
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

                VehicleViewItemsGroup vehicleItems = await FindAncestor<MainScreenViewModel>().CurrentVehicle.GetViewItemsGroupAsync();
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
                var mainScreenViewModel = FindAncestor<MainScreenViewModel>();
                Guid vehicleId = mainScreenViewModel.CurrentVehicleId;
                if (vehicleId == Guid.Empty)
                {
                    throw new Exception("VehicleId was empty");
                }

                foreach (var f in Entries)
                {
                    f.Identifier = Guid.NewGuid();
                    f.VehicleIdentifier = mainScreenViewModel.CurrentVehicleId;
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

            base.GetPopupViewModelHost().Popups.Clear();
        }

        private void ParseCsv(string csv)
        {
            try
            {
                Entries = new List<DataItemFuelEntry>();

                using (StringReader stringReader = new StringReader(csv))
                {
                    using (CsvReader reader = new CsvReader(stringReader, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        while (reader.Read())
                        {
                            // Skip header
                            if (reader.GetField(0) == "Odometer")
                            {
                                continue;
                            }

                            DataItemFuelEntry entry;

                            string[] record = new string[reader.ColumnCount];
                            for (int i = 0; i < record.Length; i++)
                            {
                                record[i] = reader.GetField(i);
                            }

                            try
                            {
                                entry = ParseRecord(record);
                            }

                            catch (ParseException p)
                            {
                                new PortableMessageDialog(p.Message + "\n\nRow: " + reader.CurrentIndex, "Error parsing CSV").ShowAsync();
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

        private View RenderCell(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(4),
                WrapText = false
            };
        }

        private View RenderVerticalLine()
        {
            return new Border
            {
                Width = 1,
                BackgroundColor = Theme.Current.AccentColor
            };
        }

        private View RenderHorizontalLine()
        {
            return new Border
            {
                Height = 1,
                BackgroundColor = Theme.Current.AccentColor
            };
        }

        private View RenderColumn(string header, IEnumerable<string> values)
        {
            var layout = new LinearLayout
            {
                Children =
                {
                    RenderHorizontalLine(),
                    RenderCell(header)
                }
            };

            foreach (var val in values)
            {
                layout.Children.Add(RenderHorizontalLine());
                layout.Children.Add(RenderCell(val));
            }

            layout.Children.Add(RenderHorizontalLine());

            return layout;
        }

        private View RenderTable()
        {
            var numbers = new List<string>();
            for (int i = 1; i <= Entries.Count; i++)
            {
                numbers.Add(i.ToString());
            }

            var horizontalLayout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin),
                Children =
                {
                    RenderVerticalLine(),
                    RenderColumn("#", numbers),
                    RenderVerticalLine(),
                    RenderColumn("Odometer", Entries.Select(i => i.Mileage.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Cost per gallon", Entries.Select(i => i.CostPerGallon.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Gallons", Entries.Select(i => i.Gallons.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Date", Entries.Select(i => i.Date.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Store name", Entries.Select(i => i.StoreName)),
                    RenderVerticalLine(),
                    RenderColumn("Fuel type", Entries.Select(i => i.FuelType.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Partial fillup", Entries.Select(i => i.PartialFill.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Skipped previous", Entries.Select(i => i.SkippedEnteringPreviousFillup.ToString())),
                    RenderVerticalLine(),
                    RenderColumn("Notes", Entries.Select(i => i.Notes)),
                    RenderVerticalLine(),
                }
            };

            return horizontalLayout;
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = RenderTable(),
                        CanScrollHorizontally = true
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new AccentButton
                            {
                                Text = "Add to records",
                                Tapped = AddToExistingRecords,
                                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, 6, Theme.Current.PageMargin + NookInsets.Bottom)
                            }.LinearLayoutWeight(1),
                            new Button
                            {
                                Text = "Replace existing records",
                                Tapped = ReplaceExistingRecords,
                                Margin = new Thickness(6, 0, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom)
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };
        }
    }
}
