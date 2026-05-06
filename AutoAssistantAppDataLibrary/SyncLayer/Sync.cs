using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantLibrary.Items;
using AutoAssistantLibrary.Requests;
using AutoAssistantLibrary.Responses;
using StorageEverywhere;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.SyncLayer
{

    public static class SyncExtensions
    {
        public static Func<string> GetPlatform;
        public static Func<string> GetAppName;
    }

    public class SyncResult
    {
        public string Error { get; set; }

        public List<SyncError> UpdateErrors { get; private set; } = new List<SyncError>();

        public SaveChangesTasks SaveChangesTask { get; internal set; }
    }

    public class SyncError
    {
        public string Name { get; private set; }
        public string Message { get; private set; }
        public DateTime Date { get; private set; }

        internal SyncError(string name, string message)
        {
            Name = name;
            Message = message;
            Date = DateTime.Now;
        }

        public override string ToString()
        {
            return Name + " - " + Date + "\n" + Message;
        }
    }

    public class SyncFinishedEventArgs
    {
        public AccountDataItem Account { get; private set; }
        public SyncResult Result { get; private set; }

        public SyncFinishedEventArgs(AccountDataItem account, SyncResult result)
        {
            Account = account;
            Result = result;
        }
    }

    public class SyncQueuedEventArgs
    {
        public AccountDataItem Account { get; private set; }
        public Task<SyncResult> ResultTask { get; private set; }

        public SyncQueuedEventArgs(AccountDataItem account, Task<SyncResult> resultTask)
        {
            Account = account;
            ResultTask = resultTask;
        }
    }

    public static class Sync
    {
        public static event EventHandler<SyncQueuedEventArgs> SyncQueued;


        private class AccountSyncRequest
        {
            public AccountDataItem Account { get; private set; }

            internal TaskCompletionSource<SyncResult> TaskCompletionSource { get; private set; } = new TaskCompletionSource<SyncResult>();

            private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public CancellationToken CancellationToken
            {
                get { return _cancellationTokenSource.Token; }
            }

            public void Cancel()
            {
                _cancellationTokenSource.Cancel();
                TaskCompletionSource.TrySetCanceled();
            }

            public AccountSyncRequest(AccountDataItem account)
            {
                Account = account;
            }
        }

        private static List<AccountSyncRequest> _queuedRequests = new List<AccountSyncRequest>();

        private static AccountSyncRequest _currTask;

        private static object _lock = new object();

        /// <summary>
        /// Places the account on the queue to be synced (will merge with existing queued if there are any), async task completes once the account is synced.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static Task<SyncResult> SyncAccountAsync(AccountDataItem account)
        {
            try
            {
                if (!account.IsOnlineAccount)
                    return new Task<SyncResult>(NotOnlineAccount);

                var task = SyncAccountAsyncHelper(account);

                try
                {
                    if (SyncQueued != null)
                        SyncQueued(null, new SyncQueuedEventArgs(account, task));
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                }

                return task;
            }

            catch (OperationCanceledException)
            {
                throw;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new Task<SyncResult>(GenericFailure);
            }
        }

        private static SyncResult GenericFailure()
        {
            return new SyncResult()
            {
                Error = "Sync failed, error info sent to developer."
            };
        }

        private static SyncResult NotOnlineAccount()
        {
            return new SyncResult()
            {
                Error = "Not online account"
            };
        }

        public static bool CancelAll()
        {
            bool canceledCurrent = false;

            lock (_lock)
            {
                // Cancel queued syncs in reverse order
                foreach (var req in _queuedRequests.Reverse<AccountSyncRequest>())
                {
                    req.Cancel();
                }

                // Then clear all of them
                _queuedRequests.Clear();

                // Then cancel/clear the current sync
                if (_currTask != null)
                {
                    _currTask.Cancel();
                    _currTask = null;
                    canceledCurrent = true;
                }
            }

            return canceledCurrent;
        }

        private static Task<SyncResult> SyncAccountAsyncHelper(AccountDataItem account)
        {
            try
            {
                AccountSyncRequest requestToExecute = null;

                lock (_lock)
                {
                    AccountSyncRequest queuedRequest = _queuedRequests.FirstOrDefault(i => i.Account == account);

                    // If there's an existing in the queue, we'll use that
                    if (queuedRequest != null)
                        return queuedRequest.TaskCompletionSource.Task;

                    var request = new AccountSyncRequest(account);

                    // If something's syncing right now
                    if (_currTask != null)
                    {
                        // Queue it up
                        _queuedRequests.Add(request);
                        return request.TaskCompletionSource.Task;
                    }

                    // Otherwise nothing is pending/syncing, so we go
                    _currTask = request;
                    requestToExecute = request;
                }

                // We execute outside of the lock (we know that it'll be initialized if it got here)
                BeginExecuteSyncRequest(requestToExecute);
                return requestToExecute.TaskCompletionSource.Task;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new Task<SyncResult>(GenericFailure);
            }
        }


        private static async void BeginExecuteSyncRequest(AccountSyncRequest request)
        {
            SyncResult result;

            try
            {
                result = await ExecuteSync(request);
            }

            catch (OperationCanceledException)
            {
                // TaskCompletionSource already set to canceled, and all requests are being cleared
                // So just do nothing
                return;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                result = GenericFailure();
            }

            // Trigger completed
            request.TaskCompletionSource.TrySetResult(result);


            bool shouldUploadImages = false;
            AccountSyncRequest nextRequest = null;

            lock (_lock)
            {
                // Handle switching to next task or image uploads
                _currTask = null;

                // If there's another sync request
                if (_queuedRequests.Count > 0)
                {
                    // Select it as the current and remove from queue
                    // It'll be executed later when I exit the lock
                    nextRequest = _queuedRequests.First();
                    _queuedRequests.RemoveAt(0);
                    _currTask = nextRequest;
                }

                // Otherwise no pending sync request
                else
                {
                    // If there was no error, we can queue image upload
                    if (result.Error == null)
                        shouldUploadImages = true;
                }
            }

            if (nextRequest != null)
                BeginExecuteSyncRequest(nextRequest);
        }

        private static async Task<SyncResult> ExecuteSync(AccountSyncRequest request)
        {
            var account = request.Account;

            try
            {
                if (account.NeedsToSyncSettings)
                {
                    // TODO: In future add support for syncing
                    //await SyncSettings(account);
                    //account.NeedsToSyncSettings = false;
                }

                string pushChannelString = null;

                //if (!account.IsPushDisabled && PushExtension.Current != null)
                //{
                //    try
                //    {
                //        pushChannelString = await PushExtension.Current.GetPushChannelUri();
                //    }

                //    catch { }

                //    request.CancellationToken.ThrowIfCancellationRequested();
                //}

                SyncRequest req;

                var accountDataStore = await AccountDataStore.Get(account.LocalAccountId);

                request.CancellationToken.ThrowIfCancellationRequested();

                var updatesAndDeletes = await accountDataStore.GetUpdatesAndDeletesAsync();

                request.CancellationToken.ThrowIfCancellationRequested();

                string pushChannelForAccount;

                pushChannelForAccount = pushChannelString;

                req = new SyncRequest()
                {
                    Credentials = account.GenerateCredentials(),
                    Vehicles = new ModifyItemsSet<AutoAssistantLibrary.Items.SyncItemVehicle>()
                    {
                        CurrentChangeNumber = account.CurrentChangeNumberVehicles,
                        Updates = updatesAndDeletes.Vehicles.Updates,
                        Deletes = updatesAndDeletes.Vehicles.Deletes
                    },
                    Fuel = new ModifyItemsSet<AutoAssistantLibrary.Items.SyncItemFuelEntry>()
                    {
                        CurrentChangeNumber = account.CurrentChangeNumberFuel,
                        Updates = updatesAndDeletes.Fuel.Updates,
                        Deletes = updatesAndDeletes.Fuel.Deletes
                    },
                    MaintenanceSchedule = new ModifyItemsSet<AutoAssistantLibrary.Items.SyncItemMaintenanceScheduleItem>()
                    {
                        CurrentChangeNumber = account.CurrentChangeNumberMaintenanceSchedule,
                        Updates = updatesAndDeletes.MaintenanceSchedule.Updates,
                        Deletes = updatesAndDeletes.MaintenanceSchedule.Deletes
                    },
                    MaintenanceRecords = new ModifyItemsSet<AutoAssistantLibrary.Items.SyncItemMaintenanceRecordEntry>()
                    {
                        CurrentChangeNumber = account.CurrentChangeNumberMaintenanceRecords,
                        Updates = updatesAndDeletes.MaintenanceRecords.Updates,
                        Deletes = updatesAndDeletes.MaintenanceRecords.Deletes
                    },
                    DeviceId = account.DeviceId,
                    Platform = SyncExtensions.GetPlatform(),
                    PushChannel = pushChannelForAccount,
                    AppName = SyncExtensions.GetAppName(),
                    AppVersion = Variables.VERSION.ToString(),
                    SyncVersion = 1,
                    MaxItemsToReturn = 200
                };

                // Perform online sync
                SyncResponse response;
                SyncResult answer = new SyncResult();
                bool isMultiPart = false;
                int responseChangeNumberVehicles = 0;
                int responseChangeNumberFuels = 0;
                int responseChangeNumberSchedule = 0;
                int responseChangeNumberRecords = 0;
                bool needsAnotherSync = updatesAndDeletes.NeedsAnotherSync;
                IEnumerable<Guid> deletedVehicles = null;
                IEnumerable<Guid> deletedFuels = null;
                IEnumerable<Guid> deletedSchedules = null;
                IEnumerable<Guid> deletedRecords = null;
                IEnumerable<SyncItemVehicle> updatedVehicles = null;
                IEnumerable<SyncItemFuelEntry> updatedFuels = null;
                IEnumerable<SyncItemMaintenanceScheduleItem> updatedSchedules = null;
                IEnumerable<SyncItemMaintenanceRecordEntry> updatedRecords = null;
                int partialSyncNumber = 0;
                IFolder partialSyncsFolder = null;
                List<IFile> partialSyncFiles = null;

                do
                {
                    try
                    {
                        response = await WebHelper.Download<SyncRequest, SyncResponse>(Website.URL + "sync", req, Website.ApiKey, request.CancellationToken);
                    }

                    catch (OperationCanceledException)
                    {
                        throw;
                    }

                    catch (Exception ex)
                    {
                        // Ignore HttpRequestException from telemetry since that just means offline
                        // WebException means things like DNS name resolution error, connection timeout, network unreachable, etc
                        if (!(ex is HttpRequestException) && !(ex is System.Net.WebException))
                        {
                            TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                            Debug.WriteLine("Error syncing (WebException): " + ex.ToString());
                        }

                        return new SyncResult()
                        {
                            Error = "Offline."
                        };
                    }

                    if (response == null)
                    {
                        Debug.WriteLine("Sync response was null");

                        return new SyncResult()
                        {
                            Error = "Sync response was null"
                        };
                    }

                    if (response.Error != null)
                    {
                        Debug.WriteLine("Sync error: " + response.Error);

                        return new SyncResult()
                        {
                            Error = response.Error
                        };
                    }

                    Debug.WriteLine("Sync response received.");

                    // Record errors about the items
                    if (response.UpdateErrors != null && response.UpdateErrors.Count > 0)
                    {
                        foreach (var updateError in response.UpdateErrors)
                            answer.UpdateErrors.Add(new SyncError("Update Item Error", updateError.ToString()));

                        Debug.WriteLine("Had " + response.UpdateErrors.Count + " UpdateErrors");
                    }

                    // Merge deletes
                    if (deletedVehicles == null)
                        deletedVehicles = response.Vehicles.DeletedItems;
                    else
                        deletedVehicles = deletedVehicles.Concat(response.Vehicles.DeletedItems);
                    if (deletedFuels == null)
                        deletedFuels = response.Fuel.DeletedItems;
                    else
                        deletedFuels = deletedFuels.Concat(response.Fuel.DeletedItems);
                    if (deletedSchedules == null)
                        deletedSchedules = response.MaintenanceSchedule.DeletedItems;
                    else
                        deletedSchedules = deletedSchedules.Concat(response.MaintenanceSchedule.DeletedItems);
                    if (deletedRecords == null)
                        deletedRecords = response.MaintenanceRecords.DeletedItems;
                    else
                        deletedRecords = deletedRecords.Concat(response.MaintenanceRecords.DeletedItems);

                    // If it's the first sync, we log the change number
                    if (!isMultiPart)
                    {
                        responseChangeNumberVehicles = response.Vehicles.ChangeNumber;
                        responseChangeNumberFuels = response.Fuel.ChangeNumber;
                        responseChangeNumberSchedule = response.MaintenanceSchedule.ChangeNumber;
                        responseChangeNumberRecords = response.MaintenanceRecords.ChangeNumber;
                    }
                    else
                    {
                        // Otherwise, if the change number has changed since the first sync,
                        // we need to request another sync after we're done to grab any item that was
                        // inserted while we were syncing
                        if (response.Vehicles.ChangeNumber > responseChangeNumberVehicles
                            || response.Fuel.ChangeNumber > responseChangeNumberFuels
                            || response.MaintenanceSchedule.ChangeNumber > responseChangeNumberSchedule
                            || response.MaintenanceRecords.ChangeNumber > responseChangeNumberRecords)
                        {
                            needsAnotherSync = true;
                        }
                    }

                    // If it's multi-part sync and this is the FIRST of the parts
                    if (response.NextPage != null && !isMultiPart)
                    {
                        Debug.WriteLine("Using multi-part sync");

                        isMultiPart = true;

                        // Clear any updates/deletes so we don't send them again
                        req.Vehicles.Updates = null;
                        req.Vehicles.Deletes = null;
                        req.Fuel.Updates = null;
                        req.Fuel.Deletes = null;
                        req.MaintenanceSchedule.Updates = null;
                        req.MaintenanceSchedule.Deletes = null;
                        req.MaintenanceRecords.Updates = null;
                        req.MaintenanceRecords.Deletes = null;

                        // Create the folder for partial syncs (deletes existing folder)
                        partialSyncsFolder = await FileHelper.CreatePartialSyncsFolder();
                        partialSyncFiles = new List<IFile>();
                    }

                    // If it's multi-part sync
                    if (response.NextPage != null)
                    {
                        // Pass the next page
                        req.Page = response.NextPage;
                    }

                    if (response.Vehicles.UpdatedItems != null)
                    {
                        // If we were in a multi-part sync, we need to save updated items to a file
                        // Note that we're disabling multi-part, need to implement logic on both server and client,
                        // shouldn't be necessary yet
                        //if (isMultiPart && (response.UpdatedItems.MegaItems.Count > 0 || response.UpdatedItems.Grades.Count > 0))
                        //{
                        //    var partialFile = await partialSyncsFolder.CreateFileAsync("Partial" + partialSyncNumber, CreationCollisionOption.ReplaceExisting);
                        //    partialSyncFiles.Add(partialFile);
                        //    var jsonSerializer = new JsonSerializer();
                        //    using (var stream = await partialFile.OpenAsync(FileAccess.ReadAndWrite))
                        //    {
                        //        using (var streamWriter = new StreamWriter(stream))
                        //        {
                        //            jsonSerializer.Serialize(streamWriter, new UpdatedItems()
                        //            {
                        //                MegaItems = response.UpdatedItems.MegaItems,
                        //                Grades = response.UpdatedItems.Grades
                        //            });
                        //        }
                        //    }
                        //}

                        // If first updated items we've received
                        if (updatedVehicles == null)
                        {
                            updatedVehicles = response.Vehicles.UpdatedItems;
                        }
                        else
                        {
                            // Otherwise we merge and cache everything
                            updatedVehicles.Concat(response.Vehicles.UpdatedItems);
                        }
                    }

                    if (response.Fuel.UpdatedItems != null)
                    {
                        if (updatedFuels == null)
                            updatedFuels = response.Fuel.UpdatedItems;
                        else
                            updatedFuels.Concat(response.Fuel.UpdatedItems);
                    }
                    if (response.MaintenanceSchedule.UpdatedItems != null)
                    {
                        if (updatedSchedules == null)
                            updatedSchedules = response.MaintenanceSchedule.UpdatedItems;
                        else
                            updatedSchedules.Concat(response.MaintenanceSchedule.UpdatedItems);
                    }
                    if (response.MaintenanceRecords.UpdatedItems != null)
                    {
                        if (updatedRecords == null)
                            updatedRecords = response.MaintenanceRecords.UpdatedItems;
                        else
                            updatedRecords.Concat(response.MaintenanceRecords.UpdatedItems);
                    }

                    // If there's no next page, we're done
                    if (response.NextPage == null)
                    {
                        break;
                    }

                    // Otherwise, we continue to make the next request (the next page has already been passed)
                    partialSyncNumber++;

                    if (partialSyncNumber > 50)
                    {
                        throw new Exception("Partial sync seems to be in an infinite loop. NextPage: " + response.NextPage);
                    }

                } while (true);

                // Mark our items as sent
                await accountDataStore.ClearSyncing();

                // Create changes so we can process the sync changes.
                // If we are NOT doing multi-part, we'll throw if item already has been added.
                // Otherwise, we won't throw, since when doing a multi-part where we had
                // updates AND are doing a re-sync, there's no way for the server to ensure that
                // it doesn't send down duplicate items that it already sent down as part of the normal items.
                DataChanges changes = CreateChangesFromSyncResponse(
                    updatedVehicles,
                    updatedFuels,
                    updatedSchedules,
                    updatedRecords,
                    deletedVehicles,
                    deletedFuels,
                    deletedSchedules,
                    deletedRecords,
                    throwIfExists: !isMultiPart);

                answer.SaveChangesTask = new SaveChangesTasks();

                // If there's actually changes, we'll process them
                if (!changes.IsEmpty())
                {
                    // Process the changes
                    answer.SaveChangesTask = await accountDataStore.ProcessOnlineChanges(changes, isMultiPart);
                }

                bool accountChanged = false;

                // If this was a multi-part sync
                if (isMultiPart)
                {
                    // Disabling multi-sync for now
                    throw new NotImplementedException("Multi-part sync disabled");

                    // We need to load and insert the mega items and grades
                    //foreach (var partialFile in partialSyncFiles)
                    //{
                    //    UpdatedItems partialItems;
                    //    using (var stream = await partialFile.OpenAsync(FileAccess.Read))
                    //    {
                    //        using (var streamReader = new StreamReader(stream))
                    //        {
                    //            using (var jsonReader = new JsonTextReader(streamReader))
                    //            {
                    //                partialItems = new JsonSerializer().Deserialize<UpdatedItems>(jsonReader);
                    //            }
                    //        }
                    //    }

                    //    // Create changes so we can process the sync changes.
                    //    // We WILL throw if exists here, since these items are all part of a single response from the
                    //    // server, which means they're guaranteed to not have conflicts.
                    //    var changesPartial = CreateChangesFromSyncResponse(partialItems, null, throwIfExists: true);

                    //    // If there's actually changes, we'll process them
                    //    if (!changesPartial.IsEmpty())
                    //    {
                    //        // Process the changes
                    //        var saveChangesTask = await accountDataStore.ProcessOnlineChanges(changesPartial, true);
                    //        if (saveChangesTask.NeedsAccountToBeSaved)
                    //        {
                    //            answer.SaveChangesTask.NeedsAccountToBeSaved = true;
                    //        }
                    //    }
                    //}

                    //var dontWait = FileHelper.DeletePartialSyncsFolder();

                    // Now we need to reset calendar, and update tiles, and update reminders, since we disabled those
                    // while doing the multi-part insert.
                    //if (account.IsAppointmentsUpToDate)
                    //{
                    //    account.IsAppointmentsUpToDate = false;
                    //    accountChanged = true;
                    //    if (AppointmentsExtension.Current != null)
                    //    {
                    //        try
                    //        {
                    //            AppointmentsExtension.Current.ResetAllIfNeeded(account, accountDataStore);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            TelemetryExtension.Current?.TrackException(ex);
                    //        }
                    //    }
                    //}

                    // Update the tiles (don't wait on it)
                    //try
                    //{
                    //    Debug.WriteLine("Updating tile notifications");
                    //    answer.SaveChangesTask.UpdateTilesTask = TilesExtension.Current?.UpdateTileNotificationsForAccountAsync(account, accountDataStore);
                    //}

                    //catch (Exception ex)
                    //{
                    //    Debug.WriteLine("Failed to update tile notifications");
                    //    TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                    //}

                    // And update reminders (don't wait on it)
                    //try
                    //{
                    //    Debug.WriteLine("Updating toast reminders");
                    //    answer.SaveChangesTask.UpdateRemindersTask = RemindersExtension.Current?.ResetReminders(account, accountDataStore);
                    //}

                    //catch (Exception ex)
                    //{
                    //    Debug.WriteLine("Failed to update toast reminders");
                    //    TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                    //}
                }

                accountChanged = accountChanged || answer.SaveChangesTask.NeedsAccountToBeSaved;

                if (account.SyncedDataVersion < AccountDataItem.CURRENT_SYNCED_DATA_VERSION)
                {
                    account.SyncedDataVersion = AccountDataItem.CURRENT_SYNCED_DATA_VERSION;
                    accountChanged = true;
                }


                // Only apply server settings if we don't have any of our own settings to upload (otherwise we might overwrite settings if the user changed them while sync was happening)
                //if (response.Settings != null && !account.NeedsToSyncSettings)
                //{
                //    if (response.Settings.GpaOption != null && account.GpaOption != response.Settings.GpaOption.Value)
                //    {
                //        account.GpaOption = response.Settings.GpaOption.Value;
                //        accountChanged = true;
                //    }

                //    if (response.Settings.WeekOneStartsOn != null && account.WeekOneStartsOn != response.Settings.WeekOneStartsOn.Value)
                //    {
                //        account.WeekOneStartsOn = response.Settings.WeekOneStartsOn.Value;
                //        accountChanged = true;
                //    }

                //    // For now we'll just return the semester ID and on initial login the login task can apply it
                //    // since we don't handle this dynamically changing while the app is already loaded.
                //    answer.SelectedSemesterId = response.Settings.SelectedSemesterId;
                //}

                //if (account.PremiumAccountExpiresOn != response.PremiumAccountExpiresOn)
                //{
                //    account.PremiumAccountExpiresOn = response.PremiumAccountExpiresOn;
                //    accountChanged = true;
                //}

                if (responseChangeNumberVehicles > 0)
                {
                    if (account.CurrentChangeNumberVehicles != responseChangeNumberVehicles)
                    {
                        account.CurrentChangeNumberVehicles = responseChangeNumberVehicles;
                        accountChanged = true;
                    }
                }
                if (responseChangeNumberFuels > 0)
                {
                    if (account.CurrentChangeNumberFuel != responseChangeNumberFuels)
                    {
                        account.CurrentChangeNumberFuel = responseChangeNumberFuels;
                        accountChanged = true;
                    }
                }
                if (responseChangeNumberSchedule > 0)
                {
                    if (account.CurrentChangeNumberMaintenanceSchedule != responseChangeNumberSchedule)
                    {
                        account.CurrentChangeNumberMaintenanceSchedule = responseChangeNumberSchedule;
                        accountChanged = true;
                    }
                }
                if (responseChangeNumberRecords > 0)
                {
                    if (account.CurrentChangeNumberMaintenanceRecords != responseChangeNumberRecords)
                    {
                        account.CurrentChangeNumberMaintenanceRecords = responseChangeNumberRecords;
                        accountChanged = true;
                    }
                }


                // If account properties was changed, save account
                if (accountChanged)
                    await AccountsManager.Save(account);

                // Log when last synced
                account.LastSyncOn = DateTime.Now;

                // Queue another sync if it's needed
                if (needsAnotherSync)
                {
                    try
                    {
                        var dontWait = SyncAccountAsync(account);
                    }
                    catch { }
                }


                return answer;
            }

            catch (OperationCanceledException) { return new SyncResult(); }

            catch (Exception ex)
            {
                Debug.WriteLine("Error syncing: " + ex.ToString());
                TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                return new SyncResult()
                {
                    Error = ex.ToString()
                };
            }
        }

        private static DataChanges CreateChangesFromSyncResponse(
            IEnumerable<SyncItemVehicle> updatedVehicles,
            IEnumerable<SyncItemFuelEntry> updatedFuels,
            IEnumerable<SyncItemMaintenanceScheduleItem> updatedSchedules,
            IEnumerable<SyncItemMaintenanceRecordEntry> updatedRecords,
            IEnumerable<Guid> deletedVehicles,
            IEnumerable<Guid> deletedFuels,
            IEnumerable<Guid> deletedSchedules,
            IEnumerable<Guid> deletedRecords,
            bool throwIfExists)
        {
            DataChanges changes = new DataChanges();

            AddChangesFromSyncResponse(changes.Vehicles, updatedVehicles, deletedVehicles, throwIfExists);
            AddChangesFromSyncResponse(changes.Fuel, updatedFuels, deletedFuels, throwIfExists);
            AddChangesFromSyncResponse(changes.MaintenanceSchedule, updatedSchedules, deletedSchedules, throwIfExists);
            AddChangesFromSyncResponse(changes.MaintenanceRecords, updatedRecords, deletedRecords, throwIfExists);

            return changes;
        }

        private static void AddChangesFromSyncResponse<D, S>(DataChanges.ScopedDataChanges<D> scopedChanges, IEnumerable<S> updatedItems, IEnumerable<Guid> deletedItems, bool throwIfExists)
            where D : BaseDataItem, new()
            where S : BaseSyncItem
        {
            if (updatedItems != null)
            {
                foreach (var u in updatedItems)
                {
                    BaseDataItem dataItem = CreateItem(u);
                    scopedChanges.Add(dataItem, throwIfExists);
                }
            }

            if (deletedItems != null)
            {
                foreach (var d in deletedItems)
                {
                    scopedChanges.DeleteItem(d, throwIfExists);
                }
            }
        }



        /// <summary>
        /// Initializes the BaseItemWin entity, gives it this account, and deserializes the data from the item. Also copies the GUID Identifier from the item into the new entity.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static BaseDataItem CreateItem(BaseSyncItem item)
        {
            BaseDataItem entity;

            if (item is SyncItemVehicle)
                entity = new DataItemVehicle();

            else if (item is SyncItemFuelEntry)
                entity = new DataItemFuelEntry();

            else if (item is SyncItemMaintenanceRecordEntry)
                entity = new DataItemMaintenanceRecordEntry();

            else if (item is SyncItemMaintenanceScheduleItem)
                entity = new DataItemMaintenanceScheduleItem();


            else
            {
                Debug.WriteLine("CreateItem, item wasn't any of the types");
                return null;
            }

            entity.Identifier = item.Identifier; //deserialize doesn't copy the identifier
            entity.Deserialize(item, null);

            return entity;
        }

        /// <summary>
        /// Returns true if currently syncing (or sync queued to happen)
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentlySyncing()
        {
            lock (_lock)
            {
                return _currTask != null || _queuedRequests.Count > 0;
            }
        }
    }
}
