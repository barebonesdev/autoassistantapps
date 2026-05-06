using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx;
using Vx.Extensions;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsListViewModel : PopupComponentViewModel
    {
        public SettingsListViewModel(BaseViewModel parent) : base(parent)
        {
            Account = FindAncestor<MainScreenViewModel>()?.CurrentAccount;
            HasAccount = Account != null;
            Title = "Settings";
        }

        public AccountDataItem Account { get; private set; }
        public bool HasAccount { get; private set; }

        public void OpenMyAccount()
        {
            ShowPopup(MyAccountViewModel.Load(Parent));
        }

        public void OpenAbout()
        {
            ShowPopup(new AboutViewModel(Parent));
        }

        protected override View Render()
        {
            var layout = new LinearLayout()
            {
                Margin = NookInsets
            };

            bool showSyncOptions = HasAccount && Account.IsOnlineAccount;

            if (showSyncOptions)
            {
                layout.Children.Add(new TextBlock
                {
                    Text = SyncStatusText,
                    Margin = new Thickness(Theme.Current.PageMargin, layout.Children.Count == 0 ? Theme.Current.PageMargin : 0, Theme.Current.PageMargin, 0),
                    WrapText = false
                });

                var syncButton = new TextButton
                {
                    Text = SyncButtonText,
                    IsEnabled = SyncButtonIsEnabled,
                    Click = StartSync,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                if (SyncHasError)
                {
                    layout.Children.Add(new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin),
                        Children =
                        {
                            syncButton,

                            new TextBlock
                            {
                                Text = "-",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(6, 0, 6, 0),
                                WrapText = false
                            }.CaptionStyle(),

                            new TextButton
                            {
                                Text = "View sync errors",
                                IsEnabled = SyncButtonIsEnabled,
                                Click = ViewSyncErrors
                            }
                        }
                    });
                }
                else
                {
                    syncButton.Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin);
                    layout.Children.Add(syncButton);
                }
            }

            if (HasAccount)
            {
                RenderOption(
                    layout,
                    MaterialDesign.MaterialDesignIcons.AccountCircle,
                    "My account",
                    "Logout or manage account",
                    OpenMyAccount);
            }

            RenderOption(
                layout,
                MaterialDesign.MaterialDesignIcons.Info,
                "About",
                "BareBones Dev",
                OpenAbout);

            return new ScrollView(layout);
        }

        public void StartSync()
        {
            FindAncestor<MainScreenViewModel>()?.SyncCurrentAccount();
        }

        public void ViewSyncErrors()
        {
            var mainScreenViewModel = FindAncestor<MainScreenViewModel>();

            if (mainScreenViewModel != null && mainScreenViewModel.HasSyncErrors)
            {
                mainScreenViewModel.ViewSyncErrors();
            }
        }

        private void RenderOption(LinearLayout layout, string icon, string title, string subtitle, Action action)
        {
            layout.Children.Add(RenderOption(icon, title, subtitle, action));
        }

        internal static View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return new ListItemButton
            {
                AltText = title,
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 12),
                    Orientation = Orientation.Horizontal,
                    Children =
                        {
                            new FontIcon
                            {
                                Glyph = icon,
                                FontSize = 40,
                                Color = Theme.Current.AccentColor
                            },

                            new LinearLayout
                            {
                                Margin = new Thickness(6, 0, 0, 0),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = title,
                                        FontWeight = FontWeights.Bold,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = subtitle,
                                        TextColor = Theme.Current.SubtleForegroundColor,
                                        WrapText = false
                                    }
                                }
                            }.LinearLayoutWeight(1)
                        }
                },
                Click = action
            };
        }

        //public void OpenPremiumVersion()
        //{
        //    PowerPlannerApp.Current.PromptPurchase(null);
        //}

        //public void OpenReminderSettings()
        //{
        //    _pagedViewModel.Navigate(new ReminderSettingsViewModel(_pagedViewModel));
        //}

        //public void OpenSyncOptions()
        //{
        //    _pagedViewModel.Navigate(new SyncOptionsViewModel(_pagedViewModel));
        //}

        //public void OpenCalendarIntegration()
        //{
        //    _pagedViewModel.Navigate(new CalendarIntegrationViewModel(_pagedViewModel));
        //}

        //public void OpenTwoWeekScheduleSettings()
        //{
        //    _pagedViewModel.Navigate(new TwoWeekScheduleSettingsViewModel(_pagedViewModel));
        //}



        private bool _initializedSyncStatus;
        private string _syncStatusText;
        public string SyncStatusText
        {
            get
            {
                if (!_initializedSyncStatus)
                {
                    _initializedSyncStatus = true;

                    var mainScreenViewModel = FindAncestor<MainScreenViewModel>();
                    if (mainScreenViewModel != null)
                    {
                        mainScreenViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(MainScreenViewModel_PropertyChanged).Handler;
                    }

                    UpdateSyncStatus();
                }

                return _syncStatusText;
            }
            set => SetProperty(ref _syncStatusText, value, nameof(SyncStatusText));
        }

        private string _syncButtonText;
        public string SyncButtonText
        {
            get => _syncButtonText;
            set => SetProperty(ref _syncButtonText, value, nameof(SyncButtonText));
        }

        private bool _syncButtonIsEnabled;
        public bool SyncButtonIsEnabled
        {
            get => _syncButtonIsEnabled;
            set => SetProperty(ref _syncButtonIsEnabled, value, nameof(SyncButtonIsEnabled));
        }

        private bool _syncHasError;
        public bool SyncHasError
        {
            get => _syncHasError;
            set => SetProperty(ref _syncHasError, value, nameof(SyncHasError));
        }

        private void UpdateSyncStatus()
        {
            var account = Account;
            var mainScreenViewModel = FindAncestor<MainScreenViewModel>();

            if (account == null)
            {
                SyncStatusText = "No account to sync";
                SyncButtonText = "Sync now";
                SyncButtonIsEnabled = false;
            }

            if (mainScreenViewModel.SyncState == MainScreenViewModel.SyncStates.Done)
            {
                SyncButtonText = "Sync now";
                SyncButtonIsEnabled = true;
            }
            else
            {
                SyncButtonText = "Syncing...";
                SyncButtonIsEnabled = false;
            }

            if (mainScreenViewModel.HasSyncErrors)
            {
                SyncHasError = true;
                SyncStatusText = "Sync error";
            }
            else if (mainScreenViewModel.IsOffline)
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = string.Format("Offline, last sync {0}", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = "Offline, couldn't sync";
                }
            }
            else
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = string.Format("Last sync {0}", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = "Sync needed";
                }
            }
        }

        private static string FriendlyLastSyncTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return DateTimeFormatterExtension.Current.FormatAsShortTime(time);
            }
            else
            {
                return time.ToString("d");
            }
        }

        private void MainScreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainScreenViewModel.SyncState):
                case nameof(MainScreenViewModel.HasSyncErrors):
                case nameof(MainScreenViewModel.IsOffline):
                    UpdateSyncStatus();
                    MarkDirty();
                    break;
            }
        }
    }
}
