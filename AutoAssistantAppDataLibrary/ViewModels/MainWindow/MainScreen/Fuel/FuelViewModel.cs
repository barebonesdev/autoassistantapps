using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Helpers;
using Vx.Views;
using AutoAssistantAppDataLibrary.Components;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class FuelViewModel : BaseMainScreenViewModelChild
    {
        public VehicleViewItemsGroup _fuelViewItemsGroup;
        public bool IsFuelLoaded { get; private set; }
        public MyObservableList<ViewItemFuelEntry> FuelEntries { get; private set; }
        public ViewItemVehicle Vehicle { get; private set; }

        private decimal _mpgAtLastRefill = Constants.NO_MPG;
        private string _mpgAtLastRefillString = "--";
        public string MpgAtLastRefillString
        {
            get { return _mpgAtLastRefillString; }
            private set { SetProperty(ref _mpgAtLastRefillString, value, nameof(MpgAtLastRefillString)); }
        }

        private decimal _mpgInLast1000Miles = Constants.NO_MPG;
        private string _mpgInLast1000MilesString = "--";
        public string MpgInLast1000MilesString
        {
            get { return _mpgInLast1000MilesString; }
            private set { SetProperty(ref _mpgInLast1000MilesString, value, nameof(MpgInLast1000MilesString)); }
        }

        private decimal _mpgInLast3000Miles = Constants.NO_MPG;
        private string _mpgInLast3000MilesString = "--";
        public string MpgInLast3000MilesString
        {
            get { return _mpgInLast3000MilesString; }
            private set { SetProperty(ref _mpgInLast3000MilesString, value, nameof(MpgInLast3000MilesString)); }
        }

        private decimal _overallMpg = Constants.NO_MPG;
        private string _overallMpgString = "--";
        public string OverallMpgString
        {
            get { return _overallMpgString; }
            private set { SetProperty(ref _overallMpgString, value, nameof(OverallMpgString)); }
        }

        private bool _manuallySetEstimatorMpg;
        private decimal _estimatorMpg = Constants.NO_MPG;
        public decimal EstimatorMpg
        {
            get { return _estimatorMpg; }
            set { SetProperty(ref _estimatorMpg, value, nameof(EstimatorMpg)); _manuallySetEstimatorMpg = true; UpdateEstimatedCost(); }
        }

        private bool _manuallySetEstimatorCostPerGallon;
        private decimal _estimatorCostPerGallon = Constants.NO_COST;
        public decimal EstimatorCostPerGallon
        {
            get { return _estimatorCostPerGallon; }
            set { SetProperty(ref _estimatorCostPerGallon, value, nameof(EstimatorCostPerGallon)); _manuallySetEstimatorCostPerGallon = true; UpdateEstimatedCost(); }
        }

        private decimal _estimatorDistance = 100;
        public decimal EstimatorDistance
        {
            get { return _estimatorDistance; }
            set { SetProperty(ref _estimatorDistance, value, nameof(EstimatorDistance)); UpdateEstimatedCost(); }
        }

        private decimal _estimatorTotalCost = Constants.NO_COST;
        public decimal EstimatorTotalCost
        {
            get { return _estimatorTotalCost; }
            private set { SetProperty(ref _estimatorTotalCost, value, nameof(EstimatorTotalCost)); }
        }

        private decimal _estimatorTotalGallons = Constants.NO_GALLONS;
        public decimal EstimatorTotalGallons
        {
            get { return _estimatorTotalGallons; }
            private set { SetProperty(ref _estimatorTotalGallons, value, nameof(EstimatorTotalGallons)); }
        }

        private string _estimatorTotalCostString = "--";
        public string EstimatorTotalCostString
        {
            get { return _estimatorTotalCostString; }
            private set { SetProperty(ref _estimatorTotalCostString, value, nameof(EstimatorTotalCostString)); }
        }

        private string _estimatorTotalGallonsString = "--";
        public string EstimatorTotalGallonsString
        {
            get { return _estimatorTotalGallonsString; }
            private set { SetProperty(ref _estimatorTotalGallonsString, value, nameof(EstimatorTotalGallonsString)); }
        }

        public FuelViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        protected override async Task LoadAsyncOverride()
        {
            await LoadFuelEntriesAsync();
        }

        private void AccountDataStore_DataChangedEvent(object sender, DataChangedEvent e)
        {
            try
            {
                // If there were fuel changes
                if (e.LocalAccountId == MainScreenViewModel.CurrentLocalAccountId && e.Fuel.HasChanges())
                {
                    PortableDispatcher.GetCurrentDispatcher().Run(UpdateFuelStats);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task LoadFuelEntriesAsync()
        {
            _fuelViewItemsGroup = await Vehicle.GetViewItemsGroupAsync();

            FuelEntries = _fuelViewItemsGroup.Fuel;

            // Watch for changes
            _fuelViewItemsGroup.OnChangesMade += new WeakEventHandler<EventArgs>(_fuelViewItemsGroup_OnChangesMade).Handler;

            UpdateFuelStats();

            IsFuelLoaded = true;
            OnPropertyChanged(nameof(IsFuelLoaded), nameof(FuelEntries));
        }

        private void _fuelViewItemsGroup_OnChangesMade(object sender, EventArgs e)
        {
            UpdateFuelStats();
        }

        private void UpdateFuelStats()
        {
            try
            {
                _overallMpg = _fuelViewItemsGroup.CalculageMpgInLastMiles(decimal.MaxValue);
                OverallMpgString = AutoAssistantStringFormatter.FormatMpg(_overallMpg);

                _mpgAtLastRefill = Constants.NO_MPG;
                if (FuelEntries.Count > 0)
                {
                    _mpgAtLastRefill = FuelEntries[0].MPG;
                }
                MpgAtLastRefillString = AutoAssistantStringFormatter.FormatMpg(_mpgAtLastRefill);

                _mpgInLast1000Miles = _fuelViewItemsGroup.CalculageMpgInLastMiles(1000);
                MpgInLast1000MilesString = AutoAssistantStringFormatter.FormatMpg(_mpgInLast1000Miles);

                _mpgInLast3000Miles = _fuelViewItemsGroup.CalculageMpgInLastMiles(3000);
                MpgInLast3000MilesString = AutoAssistantStringFormatter.FormatMpg(_mpgInLast3000Miles);

                if (!_manuallySetEstimatorMpg)
                {
                    if (_mpgInLast1000Miles == Constants.NO_MPG)
                    {
                        EstimatorMpg = 25;
                    }
                    else
                    {
                        EstimatorMpg = Math.Round(_mpgInLast1000Miles, 1);
                    }
                }
                if (!_manuallySetEstimatorCostPerGallon)
                {
                    decimal[] costPerGallons = _fuelViewItemsGroup.Fuel.Where(i => i.CostPerGallon != Constants.NO_COST).Take(5).Select(i => i.CostPerGallon).ToArray();
                    if (costPerGallons.Length == 0)
                    {
                        EstimatorCostPerGallon = 3.10m;
                    }
                    else
                    {
                        EstimatorCostPerGallon = Math.Round(costPerGallons.Average(), 2);
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void AddFuel()
        {
            MainScreenViewModel.ShowPopup(AddFuelViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewFuelEntry(ViewItemFuelEntry entry)
        {
            MainScreenViewModel.ShowPopup(new ViewFuelViewModel(MainScreenViewModel, entry));
        }

        private void UpdateEstimatedCost()
        {
            if (EstimatorMpg == Constants.NO_MPG || EstimatorCostPerGallon == Constants.NO_COST || EstimatorDistance == Constants.NO_MILES
                || EstimatorMpg == 0)
            {
                EstimatorTotalGallons = Constants.NO_GALLONS;
                EstimatorTotalCost = Constants.NO_COST;
            }

            else
            {
                EstimatorTotalGallons = EstimatorDistance / EstimatorMpg;
                EstimatorTotalCost = EstimatorTotalGallons * EstimatorCostPerGallon;
            }

            EstimatorTotalGallonsString = AutoAssistantStringFormatter.FormatGallons(EstimatorTotalGallons);
            EstimatorTotalCostString = AutoAssistantStringFormatter.FormatCost(EstimatorTotalCost);
        }

        public void ImportFuel()
        {
            MainScreenViewModel.ShowPopup(new ImportFuelIntroViewModel(MainScreenViewModel));
        }

        public void ExportFuel()
        {
            MainScreenViewModel.ShowPopup(new ExportFuelToCsvViewModel(MainScreenViewModel));
        }

        private View RenderStats(Thickness nookInsets)
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin).Combine(nookInsets),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Statistics",
                            WrapText = false
                        }.TitleStyle(),

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = MpgAtLastRefillString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 20,
                                    WrapText = false
                                },
                                new TextBlock
                                {
                                    Text = "mpg at last refill",
                                    FontSize = 20,
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(6, 0, 0, 0)
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = "Average MPG",
                            WrapText = false,
                            Margin = new Thickness(0, 12, 0, 0)
                        }.TitleStyle(),



                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = OverallMpgString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 16,
                                    WrapText = false
                                },
                                new TextBlock
                                {
                                    Text = "lifetime mpg",
                                    FontSize = 16,
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(6, 0, 0, 0)
                                }
                            }
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = MpgInLast3000MilesString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 16,
                                    WrapText = false
                                },
                                new TextBlock
                                {
                                    Text = "in last 3,000 miles",
                                    FontSize = 16,
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(6, 0, 0, 0)
                                }
                            }
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 6, 0, 0),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = MpgInLast1000MilesString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 20,
                                    WrapText = false
                                },
                                new TextBlock
                                {
                                    Text = "in last 1,000 miles",
                                    FontSize = 20,
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(6, 0, 0, 0)
                                }
                            }
                        },
                    }
                }
            };
        }

        private View RenderEstimator(Thickness nookInsets)
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin).Combine(nookInsets),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Trip Cost Estimator",
                            WrapText = false
                        }.TitleStyle(),

                        new TextBlock
                        {
                            Text = "Find out how much your next trip will cost."
                        },

                        new NumberTextBox
                        {
                            Header = "Miles",
                            PlaceholderText = "ex: 100",
                            Number = VxValue.Create<double?>((double)EstimatorDistance, v => EstimatorDistance = v == null ? 0 : (decimal)v),
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 12, 0, 0),
                            Children =
                            {
                                new NumberTextBox
                                {
                                    Header = "MPG",
                                    PlaceholderText = "ex: 31",
                                    Number = VxValue.Create<double?>((double)EstimatorMpg, v => EstimatorMpg = v == null ? 0 : (decimal)v),
                                    Margin = new Thickness(0, 0, 6, 0)
                                }.LinearLayoutWeight(1),

                                new NumberTextBox
                                {
                                    Header = "Cost per gallon",
                                    PlaceholderText = "ex: 3.10",
                                    Number = VxValue.Create<double?>((double)EstimatorCostPerGallon, v => EstimatorCostPerGallon = v == null ? 0 : (decimal)v),
                                    Margin = new Thickness(6, 0, 0, 0)
                                }
                            }
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 18, 0, 0),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = EstimatorTotalGallonsString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 24,
                                    WrapText = false
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = EstimatorTotalCostString,
                                    TextColor = Theme.Current.AccentColor,
                                    FontSize = 24,
                                    WrapText = false
                                }
                            }
                        },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "gallons",
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = "total",
                                    WrapText = false,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }
                        }
                    }
                }
            };
        }

        private View RenderFuelEntryItem(object obj)
        {
            var entry = (ViewItemFuelEntry)obj;

            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(Theme.Current.PageMargin, 3, Theme.Current.PageMargin, 3),
                Children =
                {
                    new FrameLayout
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "22.2",
                                FontSize = 32,
                                FontWeight = FontWeights.SemiLight,
                                Opacity = 0,
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = entry.MPG == Constants.NO_MILES ? "--" : entry.MPG.ToString("N1"),
                                FontSize = 32,
                                TextColor = Theme.Current.AccentColor,
                                FontWeight = FontWeights.SemiLight,
                                WrapText = false,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                        }
                    },

                    new LinearLayout
                    {
                        Margin = new Thickness(12, 0, 0, 0),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = entry.Date.ToShortDateString(),
                                FontWeight = FontWeights.SemiBold,
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = $"{entry.TotalCost.ToString("C0")} - {entry.Gallons.ToString("N1")} gallons",
                                WrapText = false
                            }
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }

        private Func<object, View> _fuelEntryItemTemplate;
        private View RenderHistory(Thickness nookInsets)
        {
            if (_fuelEntryItemTemplate == null)
            {
                _fuelEntryItemTemplate = RenderFuelEntryItem;
            }

            return new LinearLayout
            {
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + nookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + nookInsets.Right, 0),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Fuel history",
                                WrapText = false
                            }.TitleStyle().LinearLayoutWeight(1),

                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Download,
                                    FontSize = 24,
                                    Margin = new Thickness(6)
                                },
                                Click = ImportFuel,
                                TooltipText = "Import fuel history"
                            },

                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Upload,
                                    FontSize = 24,
                                    Margin = new Thickness(6)
                                },
                                Click = ExportFuel,
                                TooltipText = "Export fuel history"
                            }
                        }
                    },

                    new ListView
                    {
                        Items = FuelEntries,
                        ItemTemplate = _fuelEntryItemTemplate,
                        ItemClicked = e => ViewFuelEntry((ViewItemFuelEntry)e)
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = "+ Add fuel entry",
                        Click = AddFuel,
                        Margin = new Thickness(Theme.Current.PageMargin + nookInsets.Left, 12, Theme.Current.PageMargin + nookInsets.Right, Theme.Current.PageMargin)
                    }
                }
            };
        }

        protected override View Render()
        {
            return new TabbedComponent
            {
                Tabs = new TabItem[]
                {
                    new TabItem
                    {
                        Title = "Stats",
                        RenderContent = RenderStats
                    },
                    new TabItem
                    {
                        Title = "Estimator",
                        RenderContent = RenderEstimator
                    },
                    new TabItem
                    {
                        Title = "History",
                        RenderContent = RenderHistory
                    }
                }
            };
        }
    }
}