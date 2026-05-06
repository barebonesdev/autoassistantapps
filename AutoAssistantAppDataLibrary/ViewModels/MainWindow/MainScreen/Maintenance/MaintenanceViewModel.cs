using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using ToolsPortable;
using Vx.Views;
using System.Drawing;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class MaintenanceViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemVehicle Vehicle { get; private set; }

        private VehicleViewItemsGroup _viewItemsGroup;

        public MyObservableList<ViewItemMaintenanceScheduleItem> ScheduleItems { get; private set; }
        public MyObservableList<ViewItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; }

        private bool _hasOverdueServices;
        public bool HasOverdueServices
        {
            get { return _hasOverdueServices; }
            private set { SetProperty(ref _hasOverdueServices, value, nameof(HasOverdueServices)); }
        }

        private bool _hasNextServices;
        public bool HasNextServices
        {
            get { return _hasNextServices; }
            private set { SetProperty(ref _hasNextServices, value, nameof(HasNextServices)); }
        }

        private bool _hasFutureServices;
        public bool HasFutureServices
        {
            get { return _hasFutureServices; }
            private set { SetProperty(ref _hasFutureServices, value, nameof(HasFutureServices)); }
        }

        private bool _hasNoServices;
        public bool HasNoServices
        {
            get { return _hasNoServices; }
            private set { SetProperty(ref _hasNoServices, value, nameof(HasNoServices)); }
        }

        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> OverdueServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();
        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> NextServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();
        public MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> FutureServices { get; private set; } = new MyObservableList<ViewItemUpcomingMaintenanceScheduleItem>();
        public MyObservableList<object> UpcomingServicesWithHeaders { get; private set; } = new MyObservableList<object>();

        public MaintenanceViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;

            _ = LoadAsync();
        }

        public override bool DelayFirstRenderTillSizePresent => true;

        protected override async Task LoadAsyncOverride()
        {
            _viewItemsGroup = await Vehicle.GetViewItemsGroupAsync();
            _viewItemsGroup.OnUpcomingServicesReset += new WeakEventHandler<EventArgs>(_viewItemsGroup_OnUpcomingServicesReset).Handler;

            ScheduleItems = _viewItemsGroup.MaintenanceSchedule;
            OnPropertyChanged(nameof(ScheduleItems));

            MaintenanceRecords = _viewItemsGroup.MaintenanceRecords;
            OnPropertyChanged(nameof(MaintenanceRecords));

            ResetUpcomingSchedule();
        }

        private void _viewItemsGroup_OnUpcomingServicesReset(object sender, EventArgs e)
        {
            ResetUpcomingSchedule();
        }

        public void AddScheduleItem()
        {
            MainScreenViewModel.ShowPopup(AddScheduleItemViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewScheduleItem(ViewItemMaintenanceScheduleItem item)
        {
            MainScreenViewModel.ShowPopup(new ViewScheduleItemViewModel(MainScreenViewModel, item));
        }

        public void AddMaintenanceRecord()
        {
            MainScreenViewModel.ShowPopup(AddMaintenanceRecordViewModel.CreateForAdd(MainScreenViewModel));
        }

        public void ViewMaintenanceRecord(ViewItemMaintenanceRecordEntry entry)
        {
            MainScreenViewModel.ShowPopup(new ViewMaintenanceRecordViewModel(MainScreenViewModel, entry));
        }

        public void ViewUpcomingService(ViewItemUpcomingMaintenanceScheduleItem item)
        {
            ViewScheduleItem(item.ScheduleItem);
        }

        private void ResetUpcomingSchedule()
        {
            DateTime today = DateTime.Today;
            decimal estimatedMileage = Vehicle.EstimatedMileage;
            List<object> finalList = new List<object>();

            var desiredOverdueServices = _viewItemsGroup.UpcomingServices.Where(i => (i.DateNeededAt != Constants.NO_DATE && i.DateNeededAt.Date < today) || (i.MilesNeededAt != Constants.NO_MILES && i.MilesNeededAt < estimatedMileage)).ToList();

            MakeUpcomingServicesListLike(OverdueServices, desiredOverdueServices);
            if (desiredOverdueServices.Any())
            {
                finalList.Add("Overdue");
                finalList.AddRange(desiredOverdueServices);
            }

            var nextServices = new List<ViewItemUpcomingMaintenanceScheduleItem>();
            DateTime firstDate = Constants.NO_DATE;
            decimal firstMileage = Constants.NO_MILES;
            foreach (var service in _viewItemsGroup.UpcomingServices.Except(desiredOverdueServices))
            {
                if (nextServices.Count == 0)
                {
                    nextServices.Add(service);
                    if (service.IsDateSooner)
                    {
                        if (service.DateNeededAt == Constants.NO_DATE)
                        {
                            firstDate = Vehicle.EstimateDateOn(service.MilesNeededAt);
                        }
                        else
                        {
                            firstDate = service.DateNeededAt;
                        }

                        // Give it wiggle room of 7 days in future
                        firstDate = firstDate.AddDays(7);
                        firstMileage = Vehicle.EstimatedMileageOn(firstDate);
                    }
                    else
                    {
                        if (service.MilesNeededAt == Constants.NO_MILES)
                        {
                            firstMileage = Vehicle.EstimatedMileageOn(firstDate);
                        }
                        else
                        {
                            firstMileage = service.MilesNeededAt;
                        }

                        // Give it wiggle room of 7 days in future
                        firstMileage = firstMileage + Vehicle.EstimatedMilesPerDay * 7;
                        firstDate = Vehicle.EstimateDateOn(firstMileage);
                    }
                }
                else
                {
                    if (service.DateNeededAt != Constants.NO_DATE && service.DateNeededAt <= firstDate
                        || service.MilesNeededAt != Constants.NO_MILES && service.MilesNeededAt <= firstMileage)
                    {
                        nextServices.Add(service);
                    }
                }
            }

            MakeUpcomingServicesListLike(NextServices, nextServices);
            if (nextServices.Any())
            {
                finalList.Add("Next service");
                finalList.AddRange(nextServices);
            }

            var futureServices = _viewItemsGroup.UpcomingServices.Except(desiredOverdueServices).Except(nextServices).ToList();
            MakeUpcomingServicesListLike(FutureServices, futureServices);
            if (futureServices.Any())
            {
                finalList.Add("Future services");
                finalList.AddRange(futureServices);
            }

            UpcomingServicesWithHeaders.MakeListLike(finalList);

            HasOverdueServices = OverdueServices.Count > 0;
            HasNextServices = NextServices.Count > 0;
            HasFutureServices = FutureServices.Count > 0;
            HasNoServices = !HasOverdueServices && !HasNextServices && !HasFutureServices;
        }

        private void MakeUpcomingServicesListLike(MyObservableList<ViewItemUpcomingMaintenanceScheduleItem> mainList, IList<ViewItemUpcomingMaintenanceScheduleItem> finalList)
        {
            List<ViewItemUpcomingMaintenanceScheduleItem> intermediateList = new List<ViewItemUpcomingMaintenanceScheduleItem>(finalList.Count);

            for (int i = 0; i < finalList.Count; i++)
            {
                var finalItem = finalList[i];

                if (i == mainList.Count)
                {
                    intermediateList.Add(finalItem);
                }
                else
                {
                    bool found = false;

                    for (int x = i; x < mainList.Count; x++)
                    {
                        // If found matching schedule item
                        if (mainList[x].ScheduleItem == finalItem.ScheduleItem)
                        {
                            // Update it
                            mainList[x].Initialize(finalItem);

                            // Add it in correct location
                            intermediateList.Add(mainList[x]);

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        intermediateList.Add(finalItem);
                    }
                }
            }

            mainList.MakeListLike(intermediateList);
        }

        public void SearchRecords()
        {
            MainScreenViewModel.ShowPopup(new SearchMaintenanceRecordsViewModel(MainScreenViewModel, MainScreenViewModel.CurrentVehicle));
        }

        private class UpcomingComponent : VxComponent
        {
            public Thickness InnerNookInsets { get; set; }
            public MaintenanceViewModel MaintenanceViewModel { get; set; }

            private View RenderUpcomingItem(object obj)
            {
                float leftMargin = Theme.Current.PageMargin + NookInsets.Left;
                float rightMargin = Theme.Current.PageMargin + InnerNookInsets.Right;

                if (obj is string header)
                {
                    // UPCOMING HEADERS
                    return new TextBlock
                    {
                        Text = header,
                        TextColor = header == "Overdue" ? Color.Red : Theme.Current.ForegroundColor,
                        FontWeight = header == "Overdue" ? FontWeights.Bold : FontWeights.SemiLight,
                        Margin = new Thickness(leftMargin, Theme.Current.PageMargin, rightMargin, 0),
                        FontSize = Theme.Current.TitleFontSize,
                    };
                }

                else if (obj is ViewItemUpcomingMaintenanceScheduleItem upcoming)
                {
                    // UPCOMING SCHEDULE ITEMS
                    return new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 2, Theme.Current.PageMargin + InnerNookInsets.Right, 6),
                        Tapped = () => MaintenanceViewModel.ViewUpcomingService(upcoming),
                        Children =
                        {
                            // LEFT COLUMN
                            new LinearLayout
                            {
                                Width = 55,
                                Children =
                                {
                                    // COUNTER
                                    new TextBlock
                                    {
                                        Text = upcoming.Counter,
                                        TextAlignment = HorizontalAlignment.Center,
                                        WrapText = false,
                                        FontSize = Theme.Current.SubtitleFontSize,
                                        TextColor = upcoming.Counter.StartsWith("-") ? Color.Red : Theme.Current.AccentColor
                                    },

                                    // COUNTER TYPE
                                    new TextBlock
                                    {
                                        Text = upcoming.CounterType,
                                        TextAlignment = HorizontalAlignment.Center,
                                        WrapText = false,
                                        TextColor = Theme.Current.SubtleForegroundColor
                                    }
                                }
                            },

                            // RIGHT COLUMN
                            new LinearLayout
                            {
                                Margin = new Thickness(6, 0, 0, 0),
                                Children =
                                {
                                    // SERVICE NAME
                                    new TextBlock
                                    {
                                        Text = upcoming.ScheduleItem.Title,
                                        FontSize = Theme.Current.SubtitleFontSize,
                                        WrapText = false
                                    },

                                    // SUBTITLE
                                    new TextBlock
                                    {
                                        Text = upcoming.Subtitle,
                                        TextColor = Theme.Current.AccentColor,
                                        WrapText = false
                                    }
                                }
                            }.LinearLayoutWeight(1)
                        }
                    };
                }

                else
                {
                    throw new InvalidOperationException();
                }
            }

            private Func<object, View> _renderUpcomingItemTemplate;
            protected override View Render()
            {
                if (_renderUpcomingItemTemplate == null)
                {
                    _renderUpcomingItemTemplate = RenderUpcomingItem;
                }

                SubscribeToCollection(MaintenanceViewModel.UpcomingServicesWithHeaders);

                if (MaintenanceViewModel.UpcomingServicesWithHeaders.Count == 0)
                {
                    return new LinearLayout
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Upcoming",
                                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + InnerNookInsets.Right, 0),
                                WrapText = false
                            }.TitleStyle(),

                            new TextBlock
                            {
                                Text = "No upcoming services.",
                                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 12, Theme.Current.PageMargin + InnerNookInsets.Right, 0)
                            }
                        }
                    };
                }

                return new ListView
                {
                    Items = MaintenanceViewModel.UpcomingServicesWithHeaders,
                    ItemTemplate = _renderUpcomingItemTemplate,
                    Padding = new Thickness(0, 0, 0, Theme.Current.PageMargin)
                };
            }
        }

        private View RenderUpcoming()
        {
            return new UpcomingComponent
            {
                MaintenanceViewModel = this,
                InnerNookInsets = _innerNookInsets
            };
        }

        public class RecordsListComponent : VxComponent
        {
            private Func<object, View> _renderRecordItemTemplate;

            public Thickness InnerNookInsets { get; set; }

            public MyObservableList<ViewItemMaintenanceRecordEntry> Records { get; set; }

            public Action<ViewItemMaintenanceRecordEntry> OnRecordClicked { get; set; }

            public Thickness ListPadding { get; set; } = new Thickness();

            protected override View Render()
            {
                if (_renderRecordItemTemplate == null)
                {
                    _renderRecordItemTemplate = RenderRecordItem;
                }

                return new ListView
                {
                    Items = Records,
                    ItemTemplate = _renderRecordItemTemplate,
                    ItemClicked = r => OnRecordClicked?.Invoke((ViewItemMaintenanceRecordEntry)r),
                    Padding = ListPadding
                };
            }

            public View RenderRecordItem(object obj)
            {
                ViewItemMaintenanceRecordEntry item = (ViewItemMaintenanceRecordEntry)obj;

                return new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, 6, Theme.Current.PageMargin + NookInsets.Right, 6),
                    Children =
                {
                    new TextBlock
                    {
                        Text = item.Title,
                        FontSize = Theme.Current.SubtitleFontSize,
                        WrapText = false
                    },

                    new TextBlock
                    {
                        Text = item.Subtitle,
                        TextColor = Theme.Current.AccentColor,
                        WrapText = false,
                        FontSize = Theme.Current.CaptionFontSize
                    },

                    string.IsNullOrWhiteSpace(item.Details) ? null : new TextBlock
                    {
                        Text = item.Details,
                        WrapText = false,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        FontSize = Theme.Current.CaptionFontSize,
                        MaxLines = 1
                    }
                }
                };
            }
        }

        public class RecordsComponent : VxComponent
        {
            public Thickness InnerNookInsets { get; set; }
            public MaintenanceViewModel MaintenanceViewModel { get; set; }

            protected override View Render()
            {
                return new LinearLayout
                {
                    Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + InnerNookInsets.Right, 0),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Records",
                                WrapText = false,
                            }.TitleStyle().LinearLayoutWeight(1),

                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Search,
                                    FontSize = 20,
                                    Margin = new Thickness(6)
                                },
                                Click = MaintenanceViewModel.SearchRecords,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },

                    new RecordsListComponent
                    {
                        Records = MaintenanceViewModel.MaintenanceRecords,
                        InnerNookInsets = InnerNookInsets,
                        OnRecordClicked = r => MaintenanceViewModel.ViewMaintenanceRecord(r),
                        ListPadding = new Thickness(0, 0, 0, Theme.Current.PageMargin)
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = "+ Add record",
                        Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, 0, Theme.Current.PageMargin + InnerNookInsets.Right, Theme.Current.PageMargin),
                        Click = MaintenanceViewModel.AddMaintenanceRecord
                    }
                }
                };
            }
        }

        private View RenderRecords()
        {
            return new RecordsComponent
            {
                MaintenanceViewModel = this,
                InnerNookInsets = _innerNookInsets
            };
        }

        private class ScheduleComponent : VxComponent
        {
            public Thickness InnerNookInsets { get; set; }
            public MaintenanceViewModel MaintenanceViewModel { get; set; }

            private View RenderScheduleItem(object obj)
            {
                ViewItemMaintenanceScheduleItem item = (ViewItemMaintenanceScheduleItem)obj;

                return new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, 6, Theme.Current.PageMargin + InnerNookInsets.Right, 6),
                    Children =
                {
                    new TextBlock
                    {
                        Text = item.Title,
                        FontSize = Theme.Current.SubtitleFontSize,
                        WrapText = false
                    },

                    new TextBlock
                    {
                        Text = item.Subtitle,
                        TextColor = Theme.Current.AccentColor,
                        WrapText = false,
                        FontSize = Theme.Current.CaptionFontSize
                    },

                    string.IsNullOrWhiteSpace(item.Details) ? null : new TextBlock
                    {
                        Text = item.Details,
                        WrapText = false,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        FontSize = Theme.Current.CaptionFontSize,
                        MaxLines = 1
                    }
                }
                };
            }

            private Func<object, View> _renderScheduleItemTemplate;
            protected override View Render()
            {
                if (_renderScheduleItemTemplate == null)
                {
                    _renderScheduleItemTemplate = RenderScheduleItem;
                }

                return new LinearLayout
                {
                    Children =
                {
                    new TextBlock
                    {
                        Text = "Schedule",
                        Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, 0)
                    }.TitleStyle(),

                    new ListView
                    {
                        Items = MaintenanceViewModel.ScheduleItems,
                        ItemTemplate = _renderScheduleItemTemplate,
                        ItemClicked = obj => MaintenanceViewModel.ViewScheduleItem((ViewItemMaintenanceScheduleItem)obj),
                        Padding = new Thickness(0, 0, 0, Theme.Current.PageMargin)
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = "+ Add schedule item",
                        Margin = new Thickness(Theme.Current.PageMargin + InnerNookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin),
                        Click = MaintenanceViewModel.AddScheduleItem
                    }
                }
                };
            }
        }

        private View RenderSchedule()
        {
            return new ScheduleComponent
            {
                MaintenanceViewModel = this,
                InnerNookInsets = _innerNookInsets
            };
        }

        private bool _isCompact = true;
        private Thickness _innerNookInsets = new Thickness();

        /// <summary>
        /// 0 == overdue
        /// 1 == records
        /// 2 == schedule
        /// </summary>
        private VxState<int> _selectedIndex = new VxState<int>(0);

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            if (size.Width < 1000)
            {
                if (!_isCompact)
                {
                    _isCompact = true;
                    _innerNookInsets = NookInsets;
                    MarkDirty();
                }
            }
            else
            {
                if (_isCompact)
                {                     
                    _isCompact = false;
                    _innerNookInsets = new Thickness();
                    MarkDirty();
                }
            }
        }

        private View RenderCompactTab(int index, string title)
        {
            bool isSelected = _selectedIndex.Value == index;

            return new TransparentContentButton
            {
                Content = new FrameLayout
                {
                    Children =
                    {
                        isSelected ? new Border
                        {
                            BackgroundColor = Theme.Current.AccentColor,
                            Height = 3,
                            VerticalAlignment = VerticalAlignment.Bottom,
                        } : null,

                        new TextBlock
                        {
                            Text = title,
                            FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal,
                            TextColor = isSelected ? Theme.Current.ForegroundColor : Theme.Current.SubtleForegroundColor,
                            TextAlignment = HorizontalAlignment.Center,
                            WrapText = false,
                            Margin = new Thickness(0, 12, 0, 12)
                        }
                    }
                },
                Click = () => _selectedIndex.Value = index
            }.LinearLayoutWeight(1);
        }

        private View RenderCompact()
        {
            View content = null;
            switch (_selectedIndex.Value)
            {
                case 0:
                    content = RenderUpcoming();
                    break;
                case 1:
                    content = RenderRecords();
                    break;
                case 2:
                    content = RenderSchedule();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new LinearLayout
            {
                Children =
                {
                    content.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        Children =
                        {
                            RenderCompactTab(0, "Upcoming"),
                            RenderCompactTab(1, "Records"),
                            RenderCompactTab(2, "Schedule")
                        }
                    },

                    new Border
                    {
                        BackgroundColor = Theme.Current.SubtleForegroundColor,
                        Height = 1
                    }
                }
            };
        }

        private View RenderFull()
        {
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    RenderUpcoming().LinearLayoutWeight(1),
                    RenderRecords().LinearLayoutWeight(1),
                    RenderSchedule().LinearLayoutWeight(1)
                }
            };
        }

        protected override View Render()
        {
            return _isCompact ? RenderCompact() : RenderFull();
        }
    }
}
