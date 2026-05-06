using AutoAssistantAppDataLibrary.App;
using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.SyncLayer;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Overview;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.Settings;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using static AutoAssistantAppDataLibrary.DataLayer.NavigationManager;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class MainScreenViewModel : BaseViewModel
    {
        public enum SyncStates
        {
            Syncing,
            UploadingImages,
            Done
        }

        private SyncStates _syncState = SyncStates.Done;
        public SyncStates SyncState
        {
            get { return _syncState; }
            set
            {
                //if (value == SyncStates.Done && UploadImageProgress < 1 && UploadImageProgress > 0)
                //{
                //    value = SyncStates.UploadingImages;
                //}

                SetProperty(ref _syncState, value, nameof(SyncState));
            }
        }

        public bool IsIndeterminateSyncing
        {
            get
            {
                return GetBindedValue(this, nameof(SyncState), delegate
                {
                    return SyncState == SyncStates.Syncing;
                });
            }
        }

        public Guid CurrentLocalAccountId
        {
            get
            {
                if (CurrentAccount == null)
                    return Guid.Empty;

                return CurrentAccount.LocalAccountId;
            }
        }

        private MainScreenViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            CurrentAccount = account;

            AccountDataStore.DataChangedEvent += AccountDataStore_DataChangedEvent;
            Sync.SyncQueued += Sync_SyncQueued;
            //Sync.UploadImageProgress += Sync_UploadImageProgress;
        }

        public AccountDataItem CurrentAccount { get; private set; }

        public Guid CurrentVehicleId { get; private set; }

        private VehicleViewItemsGroup _currentVehicleViewItemsGroup;
        private ViewItemVehicle _currentVehicle;
        public ViewItemVehicle CurrentVehicle
        {
            get { return _currentVehicle; }
            private set { SetProperty(ref _currentVehicle, value, nameof(CurrentVehicle)); }
        }

        /// <summary>
        /// Required to be a valid semester ID
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <returns></returns>
        public async Task SetCurrentVehicleAsync(Guid vehicleId)
        {
            if (CurrentVehicleId != vehicleId)
            {
                await CurrentAccount.SetCurrentVehicleAsync(vehicleId);

                CurrentVehicleId = vehicleId;
                OnVehicleChanged();
                //UpdateAvailableItemsAndTriggerUpdateDisplay();
            }
        }

        //private double _uploadImageProgress;
        //public double UploadImageProgress
        //{
        //    get { return _uploadImageProgress; }
        //    set { SetProperty(ref _uploadImageProgress, value, nameof(UploadImageProgress)); }
        //}

        //private void Sync_UploadImageProgress(object sender, UploadImageProgressEventArgs e)
        //{
        //    if (CurrentAccount == null)
        //    {
        //        return;
        //    }

        //    if (e.Account.LocalAccountId == CurrentAccount.LocalAccountId)
        //    {
        //        if (e.Progress >= 1 || e.Progress == 0)
        //        {
        //            if (SyncState == SyncStates.UploadingImages)
        //            {
        //                PortableDispatcher.GetCurrentDispatcher().Run(delegate
        //                {
        //                    UploadImageProgress = 1;
        //                    SyncState = SyncStates.Done;
        //                });
        //            }
        //            else
        //            {
        //                // Silently set image progress
        //                _uploadImageProgress = 1;
        //            }
        //        }

        //        else
        //        {
        //            if (SyncState == SyncStates.Done)
        //            {
        //                PortableDispatcher.GetCurrentDispatcher().Run(delegate
        //                {
        //                    UploadImageProgress = e.Progress;
        //                    SyncState = SyncStates.UploadingImages;
        //                });
        //            }
        //            else
        //            {
        //                // Silently set image progress
        //                _uploadImageProgress = e.Progress;
        //            }
        //        }
        //    }
        //}

        private List<LoggedError> _unreadSyncErrors = new List<LoggedError>();

        private bool _hasSyncErrors;
        public bool HasSyncErrors
        {
            get { return _hasSyncErrors; }
            private set { SetProperty(ref _hasSyncErrors, value, nameof(HasSyncErrors)); }
        }

        private bool _isOffline;
        public bool IsOffline
        {
            get { return _isOffline; }
            private set { SetProperty(ref _isOffline, value, nameof(IsOffline)); }
        }

        private void SetSyncErrors(IEnumerable<LoggedError> errors)
        {
            HasSyncErrors = true;
            _unreadSyncErrors = new List<LoggedError>(errors);
        }

        private void ClearSyncErrors()
        {
            _unreadSyncErrors.Clear();
            HasSyncErrors = false;
        }

        public void ViewSyncErrors()
        {
            if (_unreadSyncErrors.Count == 0)
            {
                return;
            }

            ShowPopup(new SyncErrorsViewModel(this, _unreadSyncErrors.ToArray()));
        }

        public void ViewSettings()
        {
            ShowPopup(new SettingsListViewModel(this));
        }

        public void SyncCurrentAccount()
        {
            if (CurrentAccount != null)
            {
                try
                {
                    var dontWait = Sync.SyncAccountAsync(CurrentAccount);
                }

                catch { }
            }
        }

        private Task<SyncResult> _currSyncResultTask;
        private async void Sync_SyncQueued(object sender, SyncQueuedEventArgs e)
        {
            try
            {
                await Dispatcher.RunOrFallbackToCurrentThreadAsync(async delegate
                {
                    try
                    {
                        if (CurrentAccount != null && CurrentAccount.LocalAccountId == e.Account.LocalAccountId)
                        {
                            IsOffline = false;
                            SyncState = SyncStates.Syncing;

                            _currSyncResultTask = e.ResultTask;

                            SyncResult result;

                            try
                            {
                                result = await e.ResultTask;
                            }

                            catch (OperationCanceledException) { result = null; }

                            // If this is still the task we're considering for intermediate, then we clear intermediate (if another was queued, it wouldn't match task)
                            if (_currSyncResultTask == e.ResultTask)
                                SyncState = SyncStates.Done;
                            else
                                return;

                            // Canceled
                            if (result == null)
                                return;

                            else if (result.Error != null)
                            {
                                if (result.Error.Equals("Offline."))
                                {
                                    IsOffline = true;
                                    ClearSyncErrors();
                                }

                                else if (result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.NO_ACCOUNT))
                                {
                                    IsOffline = false;
                                    SetSyncErrors(new LoggedError[] {
                                        new LoggedError("Online account deleted", "Your online account was deleted and no longer exists.")
                                    });
                                }

                                else
                                {
                                    IsOffline = false;
                                    SetSyncErrors(new LoggedError[] {
                                        new LoggedError("Sync Error", result.Error)
                                    });
                                }

                                if (result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.INCORRECT_PASSWORD) || result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.USERNAME_CHANGED) || result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.DEVICE_NOT_FOUND))
                                {
                                    //ShowPopupUpdateCredentials(CurrentAccount);
                                    return;
                                }

                                else if (result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.DEVICE_NOT_FOUND) || result.Error.Equals(AutoAssistantLibrary.Responses.SyncResponse.NO_DEVICE))
                                {
                                    //ShowPopupUpdateCredentials(CurrentAccount, UpdateCredentialsViewModel.UpdateTypes.NoDevice);
                                }
                            }

                            else if (result.UpdateErrors != null && result.UpdateErrors.Count > 0)
                            {
                                IsOffline = false;
                                SetSyncErrors(result.UpdateErrors.Select(i => new LoggedError("Sync Item Error", i.Name + ": " + i.Message)));
                            }

                            else
                            {
                                // All good!
                                IsOffline = false;
                                ClearSyncErrors();
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex, Extensions.Telemetry.SeverityLevel.Error);
                    }
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, Extensions.Telemetry.SeverityLevel.Error);
            }
        }

        //private void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsViewModel.UpdateTypes updateType = UpdateCredentialsViewModel.UpdateTypes.Normal)
        //{
        //    ShowPopup(UpdateCredentialsViewModel.Create(this, account, updateType));
        //}

        public static async Task<MainScreenViewModel> LoadAsync(BaseViewModel parent, AccountDataItem account, bool syncAccount = true)
        {
            try
            {
                if (account == null)
                    throw new ArgumentNullException("account");

                MainScreenViewModel model = new MainScreenViewModel(parent, account);

                await model.LoadGarageViewItemsGroupAsync();

                if (account.CurrentVehicleId != Guid.Empty)
                {
                    model.CurrentVehicleId = account.CurrentVehicleId;
                    model.OnVehicleChanged();
                }

                MainMenuSelections selectedItem;

                if (model.AvailableItems.Contains(NavigationManager.MainMenuSelection))
                    selectedItem = NavigationManager.MainMenuSelection;
                else
                {
                    selectedItem = model.AvailableItems.First();
                }

                model.SelectedItem = selectedItem;

                try
                {
                    if (syncAccount)
                    {
                        var dontWait = Sync.SyncAccountAsync(account);
                    }
                }

                catch { }

                return model;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        //private static async Task<bool> CheckThatSemesterIsValidAsync(Guid localAccountId, Guid semesterId)
        //{
        //    return await Task.Run(async delegate
        //    {
        //        return await CheckThatSemesterIsValidBlocking(localAccountId, semesterId);
        //    });
        //}

        //private static async Task<bool> CheckThatSemesterIsValidBlocking(Guid localAccountId, Guid semesterId)
        //{
        //    var dataStore = await AccountDataStore.Get(localAccountId);

        //    using (await Locks.LockDataForReadAsync())
        //    {
        //        return dataStore.TableSemesters.Count(i => i.Identifier == semesterId) > 0;
        //    }
        //}

        private async void AccountDataStore_DataChangedEvent(object sender, DataChangedEvent e)
        {
            lock (_changedItemListeners)
            {
                for (int i = 0; i < _changedItemListeners.Count; i++)
                {
                    ChangedItemListener listener;
                    _changedItemListeners[i].TryGetTarget(out listener);

                    if (listener == null)
                    {
                        _changedItemListeners.RemoveAt(i);
                        i--;
                    }

                    else
                    {
                        listener.HandleDataChangedEvent(e);
                    }
                }
            }

            // If there's no semester right now, nothing needs changing
            if (CurrentVehicleId == Guid.Empty)
                return;

            // If the changes are for this account
            if (e.LocalAccountId == CurrentLocalAccountId)
            {
                try
                {
                    // We fall back to current thread since the view model should remain correct even
                    // if the view is disconnected.
                    await Dispatcher.RunOrFallbackToCurrentThreadAsync(async delegate
                    {
                        try
                        {
                            // If this semester was deleted
                            if (e.Vehicles.DeletedItems.Contains(CurrentVehicleId))
                            {
                                await SetCurrentVehicleAsync(Guid.Empty);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    });
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private List<WeakReference<ChangedItemListener>> _changedItemListeners = new List<WeakReference<ChangedItemListener>>();

        public ChangedItemListener ListenToItem(Guid identifier)
        {
            var listener = new ChangedItemListener(CurrentLocalAccountId, identifier, Dispatcher);
            _changedItemListeners.Add(new WeakReference<ChangedItemListener>(listener));
            return listener;
        }

        public class ChangedItemListener
        {
            public event EventHandler Deleted;
            private Guid _localAccountId;
            private Guid _identifier;
            private PortableDispatcher _dispatcher;

            public ChangedItemListener(Guid accountId, Guid identifier, PortableDispatcher dispatcher)
            {
                _localAccountId = accountId;
                _identifier = identifier;
                _dispatcher = dispatcher;
            }

            internal void HandleDataChangedEvent(DataChangedEvent e)
            {
                if (e.LocalAccountId == _localAccountId)
                {
                    if (e.Fuel.DeletedItems.Contains(_identifier)
                        || e.MaintenanceRecords.DeletedItems.Contains(_identifier)
                        || e.MaintenanceSchedule.DeletedItems.Contains(_identifier)
                        || e.Vehicles.DeletedItems.Contains(_identifier))
                    {
                        _dispatcher.Run(delegate
                        {
                            Deleted?.Invoke(this, new EventArgs());
                        });
                    }
                }
            }
        }

        //private void UpdateAvailableItemsAndTriggerUpdateDisplay()
        //{
        //    updateAvailableItems();
        //}

        ////void Data_OnChangesDone(object sender, EventArgs e)
        ////{
        ////    updateAvailableItems();

        ////    //if there's currently a selected class, we'll make sure it's valid
        ////    if (SelectedClass != null)
        ////    {
        ////        //if the class is no longer in the active semester, be sure to clear it (might have been deleted)
        ////        if (Store.Data.ActiveSemester == null || !Store.Data.ActiveSemester.Classes.Contains(SelectedClass))
        ////            SelectedClass = null;
        ////    }
        ////}

        private NavigationManager.MainMenuSelections _selectedItem;
        /// <summary>
        /// Will log the user out if LogIn is selected. Will set active semester to null if Years is selected.
        /// </summary>
        public NavigationManager.MainMenuSelections SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                // The actual property will be written when the page content changes
                setSelectedItem(value);
            }
        }

        private void setSelectedItem(NavigationManager.MainMenuSelections value)
        {
            if (!AvailableItems.Contains(value))
            {
                return;
            }

            // If already selected, do nothing
            if (value == SelectedItem)
                return;

            NavigationManager.MainMenuSelection = value;

            _selectedItem = value;
            UpdateSelectedContent();
            OnPropertyChanged(nameof(SelectedItem));
        }

        public BaseViewModel SelectedContent { get; private set; }

        private void UpdateSelectedContent()
        {
            if (CurrentVehicle == null)
            {
                if (SelectedContent != null)
                {
                    SelectedContent = null;
                }
            }
            else
            {
                switch (SelectedItem)
                {
                    case MainMenuSelections.Overview:
                        SelectedContent = new OverviewViewModel(this, CurrentVehicle);
                        break;
                    case MainMenuSelections.Fuel:
                        var fuelModel = new FuelViewModel(this, CurrentVehicle);
                        _ = fuelModel.LoadAsync();
                        SelectedContent = fuelModel;
                        break;
                    case MainMenuSelections.Maintenance:
                        SelectedContent = new MaintenanceViewModel(this, CurrentVehicle);
                        break;
                    default:
                        SelectedContent = null;
                        break;
                }
            }

            OnPropertyChanged(nameof(SelectedContent));
        }

        //private ViewItemClass _selectedClass;
        //public ViewItemClass SelectedClass
        //{
        //    get
        //    {
        //        if (Classes.Contains(_selectedClass))
        //            return _selectedClass;

        //        return null;
        //    }

        //    private set { SetProperty(ref _selectedClass, value, "SelectedClass"); }
        //}

        ///// <summary>
        ///// Selects the class, or if not found, searches all the semesters and switches to that semester in order to select the class
        ///// </summary>
        ///// <param name="classId"></param>
        ///// <returns></returns>
        //public async Task<bool> SelectClass(Guid classId)
        //{
        //    if (classId == Guid.Empty)
        //        return false;

        //    // If there's no classes, or none matched, we need to check other semesters
        //    if (Classes == null || !Classes.Any(i => i.Identifier == classId))
        //    {
        //        var data = await AccountDataStore.Get(CurrentLocalAccountId);

        //        // Otherwise we have to see what semester this class might be in...
        //        Guid semesterId = await data.GetSemesterIdForClassAsync(classId);

        //        if (semesterId == Guid.Empty)
        //            return false;

        //        await SetCurrentSemester(semesterId);
        //    }


        //    // Now try selecting the class
        //    if (Classes != null)
        //    {
        //        return SelectClassWithoutLoading(classId, false);
        //    }

        //    return false;
        //}

        //public void KeepBackStack()
        //{
        //    _shouldKeepBackStack = true;
        //}

        //private bool _shouldKeepBackStack;

        ///// <summary>
        ///// Call this once when navigating. Calling this resets it to false.
        ///// </summary>
        ///// <returns></returns>
        //public bool ShouldKeepBackStack()
        //{
        //    if (_shouldKeepBackStack)
        //    {
        //        _shouldKeepBackStack = false;
        //        return true;
        //    }

        //    return false;
        //}

        //private bool SelectClassWithoutLoading(Guid classId, bool allowGoingBack)
        //{
        //    if (Classes == null)
        //        return false;

        //    ViewItemClass c;

        //    if (classId == Guid.Empty)
        //        c = null;
        //    else
        //    {
        //        c = Classes.FirstOrDefault(i => i.Identifier == classId);
        //        if (c == null)
        //            return false;
        //    }

        //    _shouldKeepBackStack = allowGoingBack;

        //    NavigationManager.ClassSelection = classId;
        //    SetContent(new ClassViewModel(this, CurrentLocalAccountId, classId, DateTime.Today, CurrentSemester));
        //    return true;
        //}

        //public void SelectClassWithinSemester(ViewItemClass classToSelect, bool allowGoingBack = false)
        //{
        //    if (classToSelect == null)
        //        SelectClassWithoutLoading(Guid.Empty, allowGoingBack);
        //    else
        //        SelectClassWithoutLoading(classToSelect.Identifier, allowGoingBack);
        //}

        public GarageViewItemsGroup GarageViewItemsGroup { get; private set; }

        private async Task LoadGarageViewItemsGroupAsync()
        {
            if (GarageViewItemsGroup == null || GarageViewItemsGroup.LocalAccountId != CurrentLocalAccountId)
            {
                GarageViewItemsGroup = await GarageViewItemsGroup.LoadAsync(CurrentLocalAccountId);
                if (GarageViewItemsGroup == null)
                {
                    throw new NullReferenceException("GarageViewItemsGroup was null");
                }
            }
        }

        private async void OnVehicleChanged()
        {
            // Restore the default stored items
            NavigationManager.RestoreDefaultMemoryItems();

            CurrentVehicle = GarageViewItemsGroup?.Vehicles.FirstOrDefault(i => i.Identifier == CurrentVehicleId);

            if (CurrentVehicle == null)
            {
                _currentVehicleViewItemsGroup = null;
            }
            else
            {
                _currentVehicleViewItemsGroup = await CurrentVehicle.GetViewItemsGroupAsync();
            }

            UpdateSelectedContent();
        }

        //private void ViewModelSchedule_OnChangesOccurred(object sender, DataChangedEvent e)
        //{
        //    UpdateAvailableItemsAndTriggerUpdateDisplay();
        //}

        #region ItemLists

        private static readonly NavigationManager.MainMenuSelections[] DEFAULT_ITEMS = new NavigationManager.MainMenuSelections[]
        {
            NavigationManager.MainMenuSelections.Overview,
            NavigationManager.MainMenuSelections.Fuel,
            NavigationManager.MainMenuSelections.Maintenance
        };

        #endregion

        private bool hasNoVehicle()
        {
            return CurrentVehicleId == Guid.Empty || CurrentVehicle == null;
        }

        //private bool hasNoClasses()
        //{
        //    return hasNoSemester() || Classes.Count == 0;
        //}

        private ObservableCollection<NavigationManager.MainMenuSelections> _availableItems = new ObservableCollection<NavigationManager.MainMenuSelections>(DEFAULT_ITEMS);
        public ObservableCollection<NavigationManager.MainMenuSelections> AvailableItems
        {
            get { return _availableItems; }
        }

        /// <summary>
        /// Restores the dates to today, etc.
        /// </summary>
        private void restoreDefaultMemoryItems()
        {
            NavigationManager.RestoreDefaultMemoryItems();
        }

        /// <summary>
        /// Returns true if changes were made
        /// </summary>
        /// <param name="desired"></param>
        /// <returns></returns>
        private bool makeAvailableItemsLike(params NavigationManager.MainMenuSelections[] desired)
        {
            return IListExtensions.MakeListLike(_availableItems, desired);
        }

        //public void AddClass(bool navigateToClassAfterAdd = false, Action<DataLayer.DataItems.DataItemClass> onClassAddedAction = null)
        //{
        //    if (Classes == null)
        //    {
        //        throw new InvalidOperationException("Classes list was null");
        //    }

        //    if (CurrentVehicleId == Guid.Empty)
        //    {
        //        throw new InvalidOperationException("CurrentSemesterId was empty");
        //    }

        //    ShowPopup(AddClassViewModel.CreateForAdd(this, new AddClassViewModel.AddParameter()
        //    {
        //        Classes = Classes.ToArray(),
        //        SemesterIdentifier = CurrentVehicleId,
        //        NavigateToClassAfterAdd = navigateToClassAfterAdd,
        //        OnClassAddedAction = onClassAddedAction
        //    }));
        //}

        //public void EditClass(ViewItemClass c)
        //{
        //    ShowPopup(AddClassViewModel.CreateForEdit(this, c));
        //}

        //public void ShowItem(BaseViewItemHomeworkExam item)
        //{
        //    ShowPopup(ViewHomeworkViewModel.Create(this, item));
        //}

        //public void EditHomeworkOrExam(BaseViewItemHomeworkExam item)
        //{
        //    ShowPopup(AddHomeworkViewModel.CreateForEdit(this, new AddHomeworkViewModel.EditParameter()
        //    {
        //        Item = item
        //    }));
        //}

        //public void EditGrade(BaseViewItemHomeworkExamGrade grade, bool whatIf = false)
        //{
        //    ShowPopup(AddGradeViewModel.CreateForEdit(this, new AddGradeViewModel.EditParameter()
        //    {
        //        Item = grade,
        //        IsInWhatIfMode = whatIf
        //    }));
        //}

        //public Task DeleteItem(BaseViewItem item)
        //{
        //    return DeleteItem(item.Identifier);
        //}

        public async Task DeleteVehicle(Guid identifier)
        {
            DataChanges changes = new DataChanges();
            changes.Vehicles.DeleteItem(identifier);

            await AutoAssistantApp.Current.SaveChanges(changes);
        }

        public async Task DeleteFuel(Guid identifier)
        {
            DataChanges changes = new DataChanges();
            changes.Fuel.DeleteItem(identifier);

            await AutoAssistantApp.Current.SaveChanges(changes);
        }

        public async Task DeleteMaintenanceRecord(Guid identifier)
        {
            DataChanges changes = new DataChanges();
            changes.MaintenanceRecords.DeleteItem(identifier);

            await AutoAssistantApp.Current.SaveChanges(changes);
        }

        public async Task DeleteMaintenanceSchedule(Guid identifier)
        {
            DataChanges changes = new DataChanges();
            changes.MaintenanceSchedule.DeleteItem(identifier);

            await AutoAssistantApp.Current.SaveChanges(changes);
        }

        public void ShowGarage()
        {
            ShowPopup(new GarageViewModel(this));
        }

        private View RenderBottomTabButton(string glyph, string text, bool isSelected, Action onClick)
        {
            return new TransparentContentButton
            {
                Content = new LinearLayout
                {
                    BackgroundColor = isSelected ? Theme.Current.BackgroundAlt3Color : System.Drawing.Color.Transparent,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = glyph,
                            FontSize = 24,
                            Color = isSelected ? Theme.Current.ForegroundColor : Theme.Current.SubtleForegroundColor,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 9, 0, 0)
                        },

                        new TextBlock
                        {
                            Text = text,
                            WrapText = false,
                            TextAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 9),
                            FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal
                        }
                    }
                },
                Click = onClick
            }.LinearLayoutWeight(1);
        }

        protected override View Render()
        {
            //if (CurrentVehicle == null)
            //{
            //    return new AccentButton
            //    {
            //        Text = "Select a vehicle",
            //        HorizontalAlignment = HorizontalAlignment.Center,
            //        VerticalAlignment = VerticalAlignment.Center,
            //        Click = ShowGarage
            //    };
            //}

            // MAIN CONTENT
            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    // TOP BAR
                    new LinearLayout
                    {
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            // NICKNAME AND MILES
                            CurrentVehicle == null ? new Border().LinearLayoutWeight(1) : new LinearLayout
                            {
                                Margin = new Thickness(
                                    Theme.Current.PageMargin + NookInsets.Left,
                                    Theme.Current.PageMargin / 2 + NookInsets.Top,
                                    Theme.Current.PageMargin,
                                    Theme.Current.PageMargin / 2),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = CurrentVehicle.Nickname,
                                        FontWeight = FontWeights.SemiBold,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = $"{CurrentVehicle.EstimatedMileage.ToString("N0")} miles (estimated)",
                                        WrapText = false
                                    }
                                }
                            }.LinearLayoutWeight(1),

                            // GARAGE BUTTON
                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Garage,
                                    FontSize = 36,
                                    Margin = new Thickness(12),
                                    Color = Theme.Current.SubtleForegroundColor
                                },
                                TooltipText = "Open garage",
                                Click = ShowGarage
                            },

                            // SETTINGS BUTTON
                            new TransparentContentButton
                            {
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Settings,
                                    FontSize = 36,
                                    Margin = new Thickness(12),
                                    Color = Theme.Current.SubtleForegroundColor
                                },
                                TooltipText = "Settings",
                                Click = ViewSettings
                            }
                        }
                    },

                    // MAIN CONTENT
                    new Border
                    {
                        Content = CurrentVehicle != null ? SelectedContent : new LinearLayout
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(Theme.Current.PageMargin),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "No vehicles. Click the garage button...",
                                    TextAlignment = HorizontalAlignment.Center
                                },

                                new TransparentContentButton
                                {
                                    Content = new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Garage,
                                        FontSize = 32,
                                        Color = Theme.Current.SubtleForegroundColor
                                    },
                                    Click = ShowGarage,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                },

                                new TextBlock
                                {
                                    Text = "on the top right to open your garage and add your first vehicle.",
                                    TextAlignment = HorizontalAlignment.Center
                                }
                            }
                        }
                    }.LinearLayoutWeight(1),

                    // BOTTOM TAB BAR
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        Children =
                        {
                            RenderBottomTabButton(
                                MaterialDesign.MaterialDesignIcons.Info,
                                "Info",
                                SelectedItem == MainMenuSelections.Overview,
                                () => SelectedItem = MainMenuSelections.Overview),

                            RenderBottomTabButton(
                                MaterialDesign.MaterialDesignIcons.GasMeter,
                                "Fuel",
                                SelectedItem == MainMenuSelections.Fuel,
                                () => SelectedItem = MainMenuSelections.Fuel),

                            RenderBottomTabButton(
                                MaterialDesign.MaterialDesignIcons.CarRepair,
                                "Maintenance",
                                SelectedItem == MainMenuSelections.Maintenance,
                                () => SelectedItem = MainMenuSelections.Maintenance)
                        }
                    }
                }
            };
        }
    }
}
