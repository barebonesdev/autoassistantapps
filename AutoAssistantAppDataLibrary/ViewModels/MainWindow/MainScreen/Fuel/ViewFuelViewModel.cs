using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.Helpers;
using System.Runtime.CompilerServices;
using ToolsPortable;
using AutoAssistantAppDataLibrary.Extensions;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ViewFuelViewModel : PopupComponentViewModel
    {
        public ViewItemFuelEntry FuelEntry { get; private set; }

        public ViewFuelViewModel(MainScreenViewModel parent, ViewItemFuelEntry fuelEntry) : base(parent)
        {
            Title = "View fuel entry";
            FuelEntry = fuelEntry;
            Commands = new PopupCommand[]
            {
                PopupCommand.Edit(Edit),
                PopupCommand.DeleteWithQuickConfirm(Delete)
            };

            this.ListenToItem(fuelEntry.Identifier).Deleted += ViewFuelViewModel_Deleted;
        }

        public string MpgString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.MPG), delegate
                {
                    return AutoAssistantStringFormatter.FormatMpg(FuelEntry.MPG);
                });
            }
        }

        public string MilesString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.MilesSinceLast), delegate
                {
                    return AutoAssistantStringFormatter.FormatMilesWithText(FuelEntry.MilesSinceLast);
                });
            }
        }

        public string TotalMilesString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.Mileage), delegate
                {
                    return AutoAssistantStringFormatter.FormatMiles(FuelEntry.Mileage) + " total";
                });
            }
        }

        public string GallonsString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.Gallons), delegate
                {
                    return AutoAssistantStringFormatter.FormatGallonsWithText(FuelEntry.Gallons);
                });
            }
        }

        public string CostPerGallonString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.CostPerGallon), delegate
                {
                    return AutoAssistantStringFormatter.FormatPricePerGallonWithText(FuelEntry.CostPerGallon);
                });
            }
        }

        public string TotalCostString
        {
            get
            {
                return GetBindedValue(nameof(FuelEntry.TotalCost), delegate
                {
                    return AutoAssistantStringFormatter.FormatCost(FuelEntry.TotalCost);
                });
            }
        }

        private string GetBindedValue(string fuelEntryPropertyName, Func<string> convert, [CallerMemberName]string propertyName = null)
        {
            return GetBindedValue(FuelEntry, fuelEntryPropertyName, convert, propertyName);
        }

        private void ViewFuelViewModel_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        public void Edit()
        {
            ShowPopup(AddFuelViewModel.CreateForEdit(FindAncestor<MainScreenViewModel>(), FuelEntry));
        }

        public async void Delete()
        {
            try
            {
                await FindAncestor<MainScreenViewModel>().DeleteFuel(FuelEntry.Identifier);

                // View model automatically removed via the deleted event
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private const float LARGE_FONT_SIZE = 38;

        private View RenderMpg()
        {
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = MpgString,
                        FontSize = LARGE_FONT_SIZE,
                        TextColor = Theme.Current.AccentColor,
                        FontWeight = FontWeights.SemiLight,
                        WrapText = false
                    },

                    new TextBlock
                    {
                        Text = "mpg",
                        FontSize = LARGE_FONT_SIZE,
                        FontWeight = FontWeights.SemiLight,
                        WrapText = false,
                        Margin = new Thickness(6, 0, 0, 0),
                        TextColor = Theme.Current.SubtleForegroundColor
                    }
                },
                Margin = new Thickness(0, 0, 6, 6)
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

        private View RenderMiles()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = (FuelEntry.Mileage == Constants.NO_MILES ? "--" : FuelEntry.Mileage.ToString("N0")) + " odometer",
                        WrapText = false,
                        Margin = new Thickness(6, 0, 0, 6)
                    },

                    RenderHorizontalLine(),

                    new TextBlock
                    {
                        Text = (FuelEntry.MilesSinceLast == Constants.NO_MILES ? "--" : FuelEntry.MilesSinceLast.ToString("N0")) + " miles driven",
                        WrapText = false,
                        Margin = new Thickness(6, 6, 0, 0)
                    },
                }
            };
        }

        private View RenderGallons()
        {
            return new LinearLayout
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = (FuelEntry.Gallons == Constants.NO_GALLONS ? "--" : FuelEntry.Gallons) + " gallons",
                        WrapText = false,
                        Margin = new Thickness(0, 6, 6, 6)
                    },

                    RenderHorizontalLine(),

                    new TextBlock
                    {
                        Text = (FuelEntry.CostPerGallon == Constants.NO_COST ? "--" : FuelEntry.CostPerGallon.ToString("C2")) + " per gallon",
                        WrapText = false,
                        Margin = new Thickness(0, 6, 6, 0)
                    },
                }
            };
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        RenderMpg(),
                        RenderVerticalLine(),
                        RenderMiles().LinearLayoutWeight(1)
                    }
                },

                RenderHorizontalLine(),

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        RenderGallons(),
                        RenderVerticalLine(),
                        new TextBlock
                        {
                            Text = FuelEntry.TotalCost == Constants.NO_COST ? "--" : FuelEntry.TotalCost.ToString("C2"),
                            FontSize = LARGE_FONT_SIZE,
                            TextColor = Theme.Current.SubtleForegroundColor,
                            FontWeight = FontWeights.SemiLight,
                            WrapText = false,
                            Margin = new Thickness(6, 6, 0, 0)
                        }.LinearLayoutWeight(1)
                    }
                }

            );
        }
    }
}
