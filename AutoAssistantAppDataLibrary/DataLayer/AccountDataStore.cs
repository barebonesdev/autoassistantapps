using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using AutoAssistantAppDataLibrary.Helpers;
using AutoAssistantLibrary.Items;
using Newtonsoft.Json;
using StorageEverywhere;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    public class DeletedItems : IEnumerable<Guid>
    {
        public List<Guid> DeletedIdentifiers = new List<Guid>();

        public bool Contains(Guid identifier)
        {
            return DeletedIdentifiers.Contains(identifier);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return DeletedIdentifiers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return DeletedIdentifiers.GetEnumerator();
        }

        public void Merge(DeletedItems newer)
        {
            // There's never any conflicts since items can only be deleted once
            DeletedIdentifiers.AddRange(newer.DeletedIdentifiers);
        }
    }

    public class ChangesOnDataItem : Dictionary<DataItemProperty, object>
    {
        public Guid Identifier { get; private set; }

        public Type Type { get; private set; }

        public ChangesOnDataItem(Type type, Guid identifier)
        {
            Type = type;
            Identifier = identifier;
        }
    }

    public class SaveChangesTasks
    {
        public System.Threading.Tasks.Task UpdateTilesTask { get; internal set; }

        public System.Threading.Tasks.Task UpdateRemindersTask { get; internal set; }

        public bool NeedsAccountToBeSaved { get; internal set; }

        public AccountDataItem Account { get; internal set; }
    }

    public interface IDataChangedEventHandler
    {
        void DataChanged(AccountDataStore dataStore, DataChangedEvent e);
    }

    public class DataChanges
    {
        public ScopedDataChanges<DataItemVehicle> Vehicles { get; private set; } = new ScopedDataChanges<DataItemVehicle>();

        public ScopedDataChanges<DataItemFuelEntry> Fuel { get; private set; } = new ScopedDataChanges<DataItemFuelEntry>();

        public ScopedDataChanges<DataItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; } = new ScopedDataChanges<DataItemMaintenanceRecordEntry>();

        public ScopedDataChanges<DataItemMaintenanceScheduleItem> MaintenanceSchedule { get; private set; } = new ScopedDataChanges<DataItemMaintenanceScheduleItem>();

        public bool IsEmpty()
        {
            return Vehicles.IsEmpty()
                && Fuel.IsEmpty()
                && MaintenanceRecords.IsEmpty()
                && MaintenanceSchedule.IsEmpty();
        }

        public class ScopedDataChanges<T> where T : BaseDataItem, new()
        {
            /// <summary>
            /// Edited (or new) items
            /// </summary>
            public IEnumerable<BaseDataItem> EditedItems
            {
                get { return _storage.Where(i => i.Value != null).Select(i => i.Value); }
            }

            public IEnumerable<Guid> IdentifiersToDelete
            {
                get { return _storage.Where(i => i.Value == null).Select(i => i.Key); }
            }

            private Dictionary<Guid, BaseDataItem> _storage = new Dictionary<Guid, BaseDataItem>();

            private bool DoesGuidExist(Guid id)
            {
                return _storage.ContainsKey(id);
            }

            /// <summary>
            /// Adds a new or edited item
            /// </summary>
            /// <param name="item"></param>
            public void Add(BaseDataItem item, bool throwIfExists = true)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                if (item.Identifier == Guid.Empty)
                    throw new ArgumentException("Identifier on edited item cannot be empty.");

                if (throwIfExists)
                {
                    if (DoesGuidExist(item.Identifier))
                        throw new ArgumentException("This item has already been added.");

                    _storage.Add(item.Identifier, item);
                }
                else
                {
                    _storage[item.Identifier] = item;
                }
            }

            public void DeleteItem(Guid identifier, bool throwIfExists = true)
            {
                if (throwIfExists)
                {
                    // Ensure no conflicts
                    if (DoesGuidExist(identifier))
                        throw new ArgumentException("This item has already been added.");

                    _storage.Add(identifier, null);
                }
                else
                {
                    _storage[identifier] = null;
                }
            }

            public bool IsEmpty()
            {
                return _storage.Count == 0;
            }

            internal void ApplyToAllEditedItems(Action<BaseDataItem> action)
            {
                foreach (var edited in EditedItems)
                {
                    action(edited);
                }
            }
        }

        internal void ApplyToAllEditedItems(Action<BaseDataItem> action)
        {
            Vehicles.ApplyToAllEditedItems(action);
            Fuel.ApplyToAllEditedItems(action);
            MaintenanceRecords.ApplyToAllEditedItems(action);
            MaintenanceSchedule.ApplyToAllEditedItems(action);
        }
    }

    /// <summary>
    /// This should be used from a thread. Things in here will block the calling thread, like the locks.
    /// </summary>
    public class AccountDataStore
    {
        public class AccountApplier<T> : IEnumerable<T> where T : BaseDataItem
        {
            private AccountDataItem _account;
            private IQueryable<T> _queryable;

            public AccountApplier(AccountDataItem account, IQueryable<T> queryable)
            {
                _account = account;
                _queryable = queryable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _queryable.AsEnumerable().Select(i => ApplyAccount(i)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private T ApplyAccount(T item)
            {
                if (item != null)
                {
                    item.Account = _account;
                }
                return item;
            }

            public int Count() { return _queryable.Count(); }
            public int Count(Expression<Func<T, bool>> predExpr) { return _queryable.Count(predExpr); }
            public T ElementAt(int index) { return ApplyAccount(_queryable.AsEnumerable().ElementAt(index)); }
            public T First() { return ApplyAccount(_queryable.First()); }
            public T First(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_queryable.First(predExpr)); }
            public T FirstOrDefault() { return ApplyAccount(_queryable.FirstOrDefault()); }
            public T FirstOrDefault(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_queryable.FirstOrDefault(predExpr)); }
            public AccountApplier<T> Where(Expression<Func<T, bool>> predExpr) { return new AccountApplier<T>(_account, _queryable.Where(predExpr)); }
        }

        public AccountDataItem Account { get; private set; }

        private ChangedItems _loadedChangedItems;

        private const string WAS_UPDATED_BY_BACKGROUND_TASK = "WasUpdatedByBackground";

        /// <summary>
        /// If data was updated by background task, clears cached accounts/data, resets flag to false, and returns true.
        /// </summary>
        /// <returns></returns>
        public static bool RetrieveAndResetWasUpdatedByBackgroundTask()
        {
            if (Settings.WasUpdatedByBackgroundTask)
            {
                _dataStoreCache.Clear();
                AccountsManager.ClearCachedAccounts();
                Settings.WasUpdatedByBackgroundTask = false;
                return true;
            }

            return false;
        }

        public static void SetUpdatedByBackgroundTask()
        {
            Settings.WasUpdatedByBackgroundTask = true;
        }

        public static event EventHandler<DataChangedEvent> DataChangedEvent;
        private static List<WeakReference<IDataChangedEventHandler>> _dataChangedEventHandlers = new List<WeakReference<IDataChangedEventHandler>>();
        public static void AddDataChangedEventHandler(IDataChangedEventHandler handler)
        {
            lock (_dataChangedEventHandlers)
            {
                ClearDisposedDataChangedEventHandlers();
                _dataChangedEventHandlers.Add(new WeakReference<IDataChangedEventHandler>(handler));
            }
        }

        private static void ClearDisposedDataChangedEventHandlers()
        {
            IDataChangedEventHandler handler;
            _dataChangedEventHandlers.RemoveAll(i => !i.TryGetTarget(out handler));
        }

        private void NotifyDataChangedEventHandlers(DataChangedEvent e)
        {
            lock (_dataChangedEventHandlers)
            {
                IDataChangedEventHandler handler = null;
                foreach (var reference in _dataChangedEventHandlers.Where(i => i.TryGetTarget(out handler)))
                {
                    try
                    {
                        handler.DataChanged(this, e);
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                    }
                }
            }
        }

        [DataContract(Namespace = "")]
        public class ChangedItems
        {
            [DataContract(Namespace = "")]
            public class ScopedChangedItems
            {
                [DataMember]
                public Dictionary<Guid, ChangedPropertiesOfDataItem> _items { get; set; } = new Dictionary<Guid, ChangedPropertiesOfDataItem>();

                private ChangedPropertiesOfDataItem GetOrCreate(Guid identifier)
                {
                    ChangedPropertiesOfDataItem answer;

                    if (_items.TryGetValue(identifier, out answer))
                        return answer;

                    answer = new ChangedPropertiesOfDataItem();

                    _items[identifier] = answer;

                    return answer;
                }

                public void AddDeletedItem(Guid identifier)
                {
                    GetOrCreate(identifier).SetDeleted();
                }

                public void AddNewItem(Guid identifier)
                {
                    GetOrCreate(identifier).SetNew();
                }

                public void AddEditedItem(Guid identifier, IEnumerable<string> changedProperties)
                {
                    GetOrCreate(identifier).SetEdited(changedProperties);
                }

                /// <summary>
                /// Returns true if made changes
                /// </summary>
                /// <returns></returns>
                public bool ClearSyncing()
                {
                    bool changed = false;

                    foreach (ChangedPropertiesOfDataItem changedProperties in _items.Values)
                    {
                        if (changedProperties.ClearSyncing())
                            changed = true;
                    }

                    Guid[] emptyToRemove = _items.Where(i => i.Value.IsEmpty()).Select(i => i.Key).ToArray();


                    if (emptyToRemove.Length > 0)
                    {
                        changed = true;

                        foreach (Guid id in emptyToRemove)
                            _items.Remove(id);
                    }


                    return changed;
                }

                public bool NeedsClearSyncing()
                {
                    foreach (ChangedPropertiesOfDataItem changedProperties in _items.Values)
                    {
                        if (changedProperties.NeedsClearSyncing())
                        {
                            return true;
                        }
                    }

                    return _items.Any(i => i.Value.IsEmpty());
                }

                public Guid[] GetAllDeleted()
                {
                    return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Deleted).Select(i => i.Key).ToArray();
                }

                public Guid[] GetAllNew()
                {
                    return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.New).Select(i => i.Key).ToArray();
                }

                public Tuple<Guid, string[]>[] GetAllEdited()
                {
                    return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Edited).Select(i => new Tuple<Guid, string[]>(i.Key, i.Value.GetEditedProperties())).ToArray();
                }

                public void MarkAllSent()
                {
                    foreach (var pair in _items)
                        pair.Value.MarkSent();
                }

                public void MarkDeletesSent()
                {
                    foreach (var pair in _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Deleted))
                        pair.Value.MarkSent();
                }

                public void MarkSent(Guid id)
                {
                    ChangedPropertiesOfDataItem val;
                    if (_items.TryGetValue(id, out val))
                    {
                        val.MarkSent();
                    }
                }

                public bool IsEmpty()
                {
                    return _items.Count == 0;
                }
            }

            [DataMember]
            public ScopedChangedItems Vehicles { get; set; } = new ScopedChangedItems();

            [DataMember]
            public ScopedChangedItems Fuel { get; set; } = new ScopedChangedItems();

            [DataMember]
            public ScopedChangedItems MaintenanceRecords { get; set; } = new ScopedChangedItems();

            [DataMember]
            public ScopedChangedItems MaintenanceSchedule { get; set; } = new ScopedChangedItems();

            private ScopedChangedItems[] _allScopedItems;
            private ScopedChangedItems[] AllScopedItems
            {
                get
                {
                    if (_allScopedItems == null)
                    {
                        _allScopedItems = new ScopedChangedItems[]
                        {
                            Vehicles,
                            Fuel,
                            MaintenanceRecords,
                            MaintenanceSchedule
                        };
                    }

                    return _allScopedItems;
                }
            }

            private Guid _localAccountId;

            private ChangedItems(Guid localAccountId)
            {
                _localAccountId = localAccountId;
            }

            public ChangedItems()
            {
                // Here just so can deserialize
            }

            /// <summary>
            /// Returns true if made changes
            /// </summary>
            /// <returns></returns>
            public bool ClearSyncing()
            {
                bool changed = false;

                foreach (var item in AllScopedItems)
                {
                    if (item.ClearSyncing())
                    {
                        changed = true;
                    }
                }

                return changed;
            }

            public bool NeedsClearSyncing()
            {
                return AllScopedItems.Any(i => i.NeedsClearSyncing());
            }

            /// <summary>
            /// Saves. Caller should establish data lock
            /// </summary>
            public async System.Threading.Tasks.Task Save()
            {
                // Get the account folder
                var timeTracker = TimeTracker.Start();
                IFolder accountFolder = await FileHelper.GetOrCreateAccountFolder(_localAccountId);
                timeTracker.End(3, "ChangedItems.Save get account folder");

                // Create temp file to write to
                timeTracker = TimeTracker.Start();
                IFile tempFile = await accountFolder.CreateFileAsync(FileNames.TEMP_ACCOUNT_CHANGED_ITEMS_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                timeTracker.End(3, "ChangedItems.Save create temp file");

                // Write the data to the temp file
                timeTracker = TimeTracker.Start();
                using (Stream s = await tempFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                {
                    timeTracker.End(3, "ChangedItems.Save open stream");

                    timeTracker = TimeTracker.Start();
                    using (StreamWriter writer = new StreamWriter(s))
                    {
                        GetSerializer().Serialize(writer, this);
                    }
                    timeTracker.End(3, $"ChangedItems.Save serializing to stream.");
                }

                // Move the temp file to the actual file
                timeTracker = TimeTracker.Start();
                await tempFile.RenameAsync(FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME, NameCollisionOption.ReplaceExisting);
                timeTracker.End(3, "ChangedItems.Save renaming temp to actual");
            }

            private static Newtonsoft.Json.JsonSerializer GetSerializer()
            {
                return new Newtonsoft.Json.JsonSerializer();
            }

            private static async Task<IFile> CreateFile(Guid localAccountId)
            {
                return await FileSystem.Current.LocalStorage.CreateFileByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId), FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME, CreationCollisionOption.ReplaceExisting);
            }

            private static async Task<IFile> GetFile(Guid localAccountId)
            {
                try
                {
                    return await FileSystem.Current.LocalStorage.GetFileByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId), FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME);
                }

                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            /// <summary>
            /// Caller should establish data lock
            /// </summary>
            /// <param name="localAccountId"></param>
            /// <returns></returns>
            public static async Task<ChangedItems> Load(AccountDataStore dataStore)
            {
                if (dataStore._loadedChangedItems != null)
                {
                    return dataStore._loadedChangedItems;
                }

                var localAccountId = dataStore.LocalAccountId;

                var timeTracker = TimeTracker.Start();
                IFile file = await GetFile(localAccountId);
                timeTracker.End(3, "ChangedItems.Load GetFile");

                if (file == null)
                {
                    dataStore._loadedChangedItems = new ChangedItems(localAccountId);
                    return dataStore._loadedChangedItems;
                }

                timeTracker = TimeTracker.Start();
                using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.Read))
                {
                    timeTracker.End(3, "ChangedItems.Load OpenAsync");

                    var serializer = GetSerializer();
                    ChangedItems answer;

                    try
                    {
                        timeTracker = TimeTracker.Start();
                        using (StreamReader reader = new StreamReader(s))
                        {
                            using (var jsonReader = new JsonTextReader(reader))
                            {
                                answer = serializer.Deserialize<ChangedItems>(jsonReader);
                            }
                        }
                        timeTracker.End(3, "ChangedItems.Load read and deserialize");
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                        answer = new ChangedItems(localAccountId);
                    }

                    answer._localAccountId = localAccountId;
                    dataStore._loadedChangedItems = answer;
                    return dataStore._loadedChangedItems;
                }
            }

            public void MarkAllSent()
            {
                foreach (var set in AllScopedItems)
                {
                    set.MarkAllSent();
                }
            }

            public void MarkDeletesSent()
            {
                foreach (var set in AllScopedItems)
                {
                    set.MarkDeletesSent();
                }
            }

            public bool IsEmpty()
            {
                return AllScopedItems.All(i => i.IsEmpty());
            }

            public ChangedItems Clone()
            {
                var cloned = new ChangedItems(_localAccountId);
                for (int i = 0; i < AllScopedItems.Length; i++)
                {
                    cloned.AllScopedItems[i]._items = new Dictionary<Guid, DataLayer.ChangedPropertiesOfDataItem>(this.AllScopedItems[i]._items);
                }
                return cloned;
            }
        }

        public Guid LocalAccountId
        {
            get { return Account.LocalAccountId; }
        }
        public AutoAssistantDbContext _db;

        private static WeakReferenceCache<Guid, AccountDataStore> _dataStoreCache = new WeakReferenceCache<Guid, AccountDataStore>();

        public static async Task<AccountDataStore> Get(Guid localAccountId)
        {
            Debug.WriteLine("AccountDataStore Get: " + localAccountId.ToString());

            // Check if already loaded
            AccountDataStore existing = GetCached(localAccountId);

            // If already loaded, yay done!
            if (existing != null)
                return existing;

            AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);
            if (account == null)
            {
                throw new Exception("Account doesn't exist");
            }

            // Otherwise will need to load, so run a task for the data lock
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                // Establish data lock (Write because we might be initializing database)
                using (await Locks.LockDataForWriteAsync("AccountDataStore.Get"))
                {
                    // Check if we've already loaded it in the meantime
                    AccountDataStore dataStore = GetCached(localAccountId);

                    // If we've already loaded it, then yay done!
                    if (dataStore != null)
                        return dataStore;

                    // Otherwise need to load it
#if DEBUG
                    Debug.WriteLine("Initializing new data store: " + localAccountId);
                    DateTime start = DateTime.Now;
#endif
                    try
                    {
                        dataStore = new AccountDataStore(account);
                        await dataStore.InitializeDatabaseAsync();
                    }
                    catch (Exception ex) when (!(ex is OutOfMemoryException))
                    {
                        // Database corrupted, delete and re-create
                        var file = await FileSystem.Current.GetFileFromPathAsync(GetDatabaseFilePath(localAccountId));
                        if (file == null)
                        {
                            throw;
                        }

                        // Need to reset the account's change number, so it re-syncs everything
                        account.CurrentChangeNumberFuel = 0;
                        account.CurrentChangeNumberMaintenanceRecords = 0;
                        account.CurrentChangeNumberMaintenanceSchedule = 0;
                        account.CurrentChangeNumberVehicles = 0;
                        await AccountsManager.Save(account);

                        // Delete the database
                        await file.DeleteAsync();

                        // Re-create
                        dataStore = new AccountDataStore(account);
                        await dataStore.InitializeDatabaseAsync();

                        TelemetryExtension.Current?.TrackEvent("DatabaseCorruptAndReset");
                    }
#if DEBUG
                    DateTime end = DateTime.Now;
                    Debug.WriteLine("Initialized new data store: " + localAccountId + ". " + (end - start).TotalMilliseconds + " milliseconds");
#endif

                    // And then set it as cached
                    lock (_dataStoreCache)
                    {
                        _dataStoreCache[localAccountId] = dataStore;
                    }

                    return dataStore;
                }

            });
        }

        private static AccountDataStore GetCached(Guid localAccountId)
        {
            lock (_dataStoreCache)
            {
                AccountDataStore existing;

                if (_dataStoreCache.TryGetValue(localAccountId, out existing) && existing != null)
                {
                    Debug.WriteLine("Returning cached data store: " + localAccountId);
                    return existing;
                }
            }

            return null;
        }

        /// <summary>
        /// Trashes the DB connection so that we can delete everything
        /// </summary>
        /// <param name="localAccountId"></param>
        public static void Dispose(Guid localAccountId)
        {
            lock (_dataStoreCache)
            {
                AccountDataStore existing;

                if (_dataStoreCache.TryGetValue(localAccountId, out existing))
                {
                    if (existing._db != null)
                    {
                        existing._db.Dispose();
                        existing._db = null;
                    }

                    _dataStoreCache.Remove(localAccountId);
                }
            }
        }

        /// <summary>
        /// Creates a data lock, saves changes
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task ClearSyncing()
        {
            await System.Threading.Tasks.Task.Run(async delegate
            {
                ChangedItems changes;
                using (await Locks.LockDataForReadAsync())
                {
                    changes = await ChangedItems.Load(this);

                    // If nothing changed, just return
                    if (!changes.NeedsClearSyncing())
                        return;
                }

                // Otherwise need to clear and save changes
                using (await Locks.LockDataForWriteAsync("AccountDataStore.ClearSyncing"))
                {
                    if (changes.ClearSyncing())
                    {
                        await changes.Save();
                    }
                }
            });
        }

        /// <summary>
        /// Caller must establish data lock. Caller must call InitializeDatabaseAsync().
        /// </summary>
        private AccountDataStore(AccountDataItem account)
        {
            Account = account;
        }

        public string DatabaseFilePath
        {
            get { return GetDatabaseFilePath(LocalAccountId); }
        }

        public static string GetDatabaseFilePath(Guid localAccountId)
        {
            return Path.Combine(FileSystem.Current.LocalStorage.Path, Path.Combine(FileNames.ACCOUNT_FOLDER_PATH(localAccountId)), FileNames.ACCOUNT_DATABASE_FILE_NAME);
        }

        /// <summary>
        /// Caller should already have data lock
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task InitializeDatabaseAsync()
        {
            try
            {
                await InitializeDatabaseHelperAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                if (_db != null)
                {
                    try
                    {
                        _db.Dispose();
                    }
                    catch { }
                    _db = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Caller must establish data lock
        /// </summary>
        private async System.Threading.Tasks.Task InitializeDatabaseHelperAsync()
        {
            var timeTracker = TimeTracker.Start();
            _db = new AutoAssistantDbContext(DatabaseFilePath, Account);

            // EnsureCreated creates all tables if the database doesn't exist,
            // and is a no-op if the database already exists (it does NOT update schema).
            // This matches the old CreateTable behavior which was also a no-op for existing tables.
            _db.Database.EnsureCreated();
            timeTracker.End(3, "AccountDataStore.InitializeDatabase create DbContext and EnsureCreated");

            // Handle upgrading data
            timeTracker = TimeTracker.Start();
            var dataInfo = _db.DataInfos.FirstOrDefault();
            if (dataInfo == null)
            {
                // If not found, have to assume we came from version 1
                // That means newly created accounts will start at version 1
                // but that's ok since the upgrade operations won't do anything
                dataInfo = new DataInfo()
                {
                    Version = 1
                };
            }
            var version = dataInfo.Version;
            if (version < DataInfo.LATEST_VERSION)
            {
                dataInfo.Version = DataInfo.LATEST_VERSION;

                // Upsert: check if tracked or in DB, then add or update
                var existingInDb = _db.DataInfos.Find(dataInfo.Key);
                if (existingInDb != null)
                {
                    existingInDb.Version = dataInfo.Version;
                }
                else
                {
                    _db.DataInfos.Add(dataInfo);
                }
                _db.SaveChanges();
            }
            timeTracker.End(3, "AccountDataStore.InitializeDatabase handle upgrade");
        }

        public class DataInfo
        {
            public const int LATEST_VERSION = 1;

            [Key]
            public short Key { get; set; } = 1;

            public int Version { get; set; }

            public static DataInfo CreateNew()
            {
                return new DataInfo()
                {
                    Version = LATEST_VERSION
                };
            }
        }

        ~AccountDataStore()
        {
            if (_db != null)
            {
                try
                {
                    _db.Dispose();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                _db = null;
            }
        }

        public AccountApplier<DataItemVehicle> TableVehicles
        {
            get { return new AccountApplier<DataItemVehicle>(Account, _db.Vehicles); }
        }

        public AccountApplier<DataItemFuelEntry> TableFuel
        {
            get { return new AccountApplier<DataItemFuelEntry>(Account, _db.FuelEntries); }
        }

        public AccountApplier<DataItemMaintenanceRecordEntry> TableMaintenanceRecords
        {
            get { return new AccountApplier<DataItemMaintenanceRecordEntry>(Account, _db.MaintenanceRecordEntries); }
        }

        public AccountApplier<DataItemMaintenanceScheduleItem> TableMaintenanceSchedules
        {
            get { return new AccountApplier<DataItemMaintenanceScheduleItem>(Account, _db.MaintenanceScheduleItems); }
        }

        public IQueryable<DataItemVehicle> ActualTableVehicles
        {
            get { return _db.Vehicles; }
        }

        public IQueryable<DataItemFuelEntry> ActualTableFuel
        {
            get { return _db.FuelEntries; }
        }

        public IQueryable<DataItemMaintenanceRecordEntry> ActualTableMaintenanceRecords
        {
            get { return _db.MaintenanceRecordEntries; }
        }

        public IQueryable<DataItemMaintenanceScheduleItem> ActualTableMaintenanceSchedule
        {
            get { return _db.MaintenanceScheduleItems; }
        }

        public enum ProcessType
        {
            Local,
            Online,
            OnlineMultiPart
        }

        public class UpdatesAndDeletes
        {
            public ScopedUpdatesAndDeletes<SyncItemVehicle> Vehicles { get; private set; } = new ScopedUpdatesAndDeletes<SyncItemVehicle>();

            public ScopedUpdatesAndDeletes<SyncItemFuelEntry> Fuel { get; private set; } = new ScopedUpdatesAndDeletes<SyncItemFuelEntry>();

            public ScopedUpdatesAndDeletes<SyncItemMaintenanceRecordEntry> MaintenanceRecords { get; private set; } = new ScopedUpdatesAndDeletes<SyncItemMaintenanceRecordEntry>();

            public ScopedUpdatesAndDeletes<SyncItemMaintenanceScheduleItem> MaintenanceSchedule { get; private set; } = new ScopedUpdatesAndDeletes<SyncItemMaintenanceScheduleItem>();

            public bool NeedsAnotherSync { get; set; }

            public class ScopedUpdatesAndDeletes<S> where S : BaseSyncItem
            {
                public List<S> Updates { get; private set; } = new List<S>();

                public Guid[] Deletes { get; set; }
            }
        }

        /// <summary>
        /// This establishes a data lock
        /// </summary>
        /// <returns></returns>
        public async Task<UpdatesAndDeletes> GetUpdatesAndDeletesAsync()
        {
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                UpdatesAndDeletes answer = new UpdatesAndDeletes();

                using (await Locks.LockDataForWriteAsync())
                {
                    ChangedItems changedItems = await ChangedItems.Load(this);

                    // If nothing changed
                    if (changedItems.IsEmpty())
                    {
                        answer.Vehicles.Deletes = new Guid[0];
                        answer.Fuel.Deletes = new Guid[0];
                        answer.MaintenanceRecords.Deletes = new Guid[0];
                        answer.MaintenanceSchedule.Deletes = new Guid[0];

                        return answer;
                    }

                    // Copy deletes
                    answer.Vehicles.Deletes = changedItems.Vehicles.GetAllDeleted();
                    answer.Fuel.Deletes = changedItems.Fuel.GetAllDeleted();
                    answer.MaintenanceRecords.Deletes = changedItems.MaintenanceRecords.GetAllDeleted();
                    answer.MaintenanceSchedule.Deletes = changedItems.MaintenanceSchedule.GetAllDeleted();

                    // grab all new/edited
                    bool needsAnotherSync = false;
                    var timeTracker = TimeTracker.Start();
                    GetUpdatesBlocking(changedItems, answer, ref needsAnotherSync);
                    timeTracker.End(3, $"AccountDataStore.GetUpdatesandDeletesAsync GetUpdatesBlocking");

                    // Mark all deletes as sent, so that when sync completes, we can remove them
                    // Updates/new were already set in GetUpdatesBlocking
                    changedItems.MarkDeletesSent();

                    // And then save that
                    await changedItems.Save();

                    answer.NeedsAnotherSync = needsAnotherSync;
                }


                return answer;
            });
        }

        /// <summary>
        /// Caller should establish data lock. This also marks all sent, so that when sync completes, we can remove them
        /// </summary>
        /// <param name="newItemIdentifiers"></param>
        /// <param name="editedItems"></param>
        /// <returns></returns>
        private void GetUpdatesBlocking(ChangedItems changedItems, UpdatesAndDeletes intoUpdatesAndDeletes, ref bool needsAnotherSync)
        {
            int remainingAllowed = 100;

            GetUpdatesBlocking(changedItems.Vehicles, intoUpdatesAndDeletes.Vehicles, ref remainingAllowed, ref needsAnotherSync);
            GetUpdatesBlocking(changedItems.Fuel, intoUpdatesAndDeletes.Fuel, ref remainingAllowed, ref needsAnotherSync);
            GetUpdatesBlocking(changedItems.MaintenanceRecords, intoUpdatesAndDeletes.MaintenanceRecords, ref remainingAllowed, ref needsAnotherSync);
            GetUpdatesBlocking(changedItems.MaintenanceSchedule, intoUpdatesAndDeletes.MaintenanceSchedule, ref remainingAllowed, ref needsAnotherSync);
        }

        private void GetUpdatesBlocking<S>(ChangedItems.ScopedChangedItems changedItems, UpdatesAndDeletes.ScopedUpdatesAndDeletes<S> intoUpdatesAndDeletes, ref int remainingAllowed, ref bool needsAnotherSync) where S : BaseSyncItem
        {
            if (needsAnotherSync)
            {
                return;
            }

            var editedItems = changedItems.GetAllEdited();

            foreach (var item in FindAll<S>(changedItems.GetAllNew().Union(editedItems.Select(i => i.Item1)).ToArray()))
            {
                if (remainingAllowed <= 0)
                {
                    needsAnotherSync = true;
                    return;
                }

                string[] changedProperties = editedItems.Where(i => i.Item1 == item.Identifier).Select(i => i.Item2).FirstOrDefault();

                // If edited, get properties selectively
                if (changedProperties != null)
                {
                    intoUpdatesAndDeletes.Updates.Add((S)item.SerializeOnlyChangedProperties(changedProperties));
                }

                // Else new item
                else
                {
                    intoUpdatesAndDeletes.Updates.Add((S)item.Serialize());
                }

                // Mark sent
                changedItems.MarkSent(item.Identifier);

                remainingAllowed--;
            }
        }

        /// <summary>
        /// Establishes data lock. Caller is responsible for checking the NeedsAccountToBeSaved property and saving the account
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<SaveChangesTasks> ProcessOnlineChanges(DataChanges dataChanges, bool isMultiPart)
        {
            return await ProcessChangesHelper(
                dataChanges,
                isMultiPart ? ProcessType.OnlineMultiPart : ProcessType.Online);
        }

        public async System.Threading.Tasks.Task<SaveChangesTasks> ProcessLocalChanges(DataChanges dataChanges)
        {
            var saveChangesTask = await ProcessChangesHelper(dataChanges, ProcessType.Local);

            if (saveChangesTask.NeedsAccountToBeSaved)
            {
                await AccountsManager.Save(saveChangesTask.Account);
                saveChangesTask.NeedsAccountToBeSaved = false;
            }

            return saveChangesTask;
        }



        /// <summary>
        /// Items in the DataChanges will be modified.
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<SaveChangesTasks> ProcessChangesHelper(DataChanges dataChanges, ProcessType processType)
        {
            SaveChangesTasks pendingTasks = new SaveChangesTasks();

            var account = await AccountsManager.GetOrLoad(LocalAccountId);
            pendingTasks.Account = account;

            await System.Threading.Tasks.Task.Run(async delegate
            {
                using (await Locks.LockDataForWriteAsync())
                {
                    var commitChangesResponse = await CommitChanges(account, dataChanges, processType);

                    pendingTasks.NeedsAccountToBeSaved = commitChangesResponse.NeedsAccountToBeSaved;
                }
            });


            // If we're not in a multi-part insert...
            if (processType != ProcessType.OnlineMultiPart)
            {
            }

            return pendingTasks;
        }

        /// <summary>
        /// Used for upgrading from Windows 8, no need for data lock since we're not going to be calling this in a multi-threaded fashion.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        internal async System.Threading.Tasks.Task ImportItemsAsync(BaseDataItem[] items)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                ImportItemsBlocking(items);
            });
        }

        private void ImportItemsBlocking(BaseDataItem[] items)
        {
            AddItemsToDbSets(items);
            _db.SaveChanges();
        }

        /// <summary>
        /// Adds items to the appropriate DbSet based on their type
        /// </summary>
        private void AddItemsToDbSets(IEnumerable<BaseDataItem> items)
        {
            foreach (var item in items)
            {
                switch (item)
                {
                    case DataItemVehicle v: _db.Vehicles.Add(v); break;
                    case DataItemFuelEntry f: _db.FuelEntries.Add(f); break;
                    case DataItemMaintenanceScheduleItem s: _db.MaintenanceScheduleItems.Add(s); break;
                    case DataItemMaintenanceRecordEntry r: _db.MaintenanceRecordEntries.Add(r); break;
                }
            }
        }

        private class CommitChangesResponse
        {
            public bool NeedsAccountToBeSaved { get; set; }
        }

        /// <summary>
        /// Items in the DataChanges will be modified.
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<CommitChangesResponse> CommitChanges(AccountDataItem account, DataChanges dataChanges, ProcessType processType)
        {
            if (dataChanges == null)
                throw new ArgumentNullException("dataChanges");

            // Assign account to items
            dataChanges.ApplyToAllEditedItems((editedItem) =>
            {
                editedItem.Account = Account;
            });

            ChangedItems changedItems = null;

            // Obtain the current changes if local request
            if (processType == ProcessType.Local)
                changedItems = await ChangedItems.Load(this);

            // Obtain the existing data items (whichever match)
            var timeTracker = TimeTracker.Start();
            List<BaseDataItem> existingVehicles = GetExistingItems<DataItemVehicle>(dataChanges.Vehicles.EditedItems);
            List<BaseDataItem> existingFuel = GetExistingItems<DataItemFuelEntry>(dataChanges.Fuel.EditedItems);
            List<BaseDataItem> existingMaintenanceRecords = GetExistingItems<DataItemMaintenanceRecordEntry>(dataChanges.MaintenanceRecords.EditedItems);
            List<BaseDataItem> existingMaintenanceSchedule = GetExistingItems<DataItemMaintenanceScheduleItem>(dataChanges.MaintenanceSchedule.EditedItems);
            timeTracker.End(3, delegate { return $"CommitChanges GetExistingItems"; });

            // Whichever didn't exist will be new items
            List<BaseDataItem> newVehicles = new List<BaseDataItem>();
            List<BaseDataItem> newFuel = new List<BaseDataItem>();
            List<BaseDataItem> newMaintenanceRecords = new List<BaseDataItem>();
            List<BaseDataItem> newMaintenanceSchedule = new List<BaseDataItem>();

            DateTime now = DateTime.UtcNow;

            // Then write the new/edited items
            ImportEditedItems(
                scopedDataChanges: dataChanges.Vehicles,
                processType: processType,
                now: ref now,
                existingDataItems: existingVehicles,
                newDataItems: newVehicles,
                scopedChanges: changedItems?.Vehicles);
            ImportEditedItems(
                scopedDataChanges: dataChanges.Fuel,
                processType: processType,
                now: ref now,
                existingDataItems: existingFuel,
                newDataItems: newFuel,
                scopedChanges: changedItems?.Fuel);
            ImportEditedItems(
                scopedDataChanges: dataChanges.MaintenanceRecords,
                processType: processType,
                now: ref now,
                existingDataItems: existingMaintenanceRecords,
                newDataItems: newMaintenanceRecords,
                scopedChanges: changedItems?.MaintenanceRecords);
            ImportEditedItems(
                scopedDataChanges: dataChanges.MaintenanceSchedule,
                processType: processType,
                now: ref now,
                existingDataItems: existingMaintenanceSchedule,
                newDataItems: newMaintenanceSchedule,
                scopedChanges: changedItems?.MaintenanceSchedule);

            // And flag the items to delete
            if (changedItems != null)
            {
                foreach (Guid id in dataChanges.Vehicles.IdentifiersToDelete)
                    changedItems.Vehicles.AddDeletedItem(id);
                foreach (Guid id in dataChanges.Fuel.IdentifiersToDelete)
                    changedItems.Fuel.AddDeletedItem(id);
                foreach (Guid id in dataChanges.MaintenanceRecords.IdentifiersToDelete)
                    changedItems.MaintenanceRecords.AddDeletedItem(id);
                foreach (Guid id in dataChanges.MaintenanceSchedule.IdentifiersToDelete)
                    changedItems.MaintenanceSchedule.AddDeletedItem(id);
            }

            DataChangedEvent dataChangedEvent = new DataChangedEvent(LocalAccountId);

            dataChangedEvent.Vehicles.NewItems.AddRange(newVehicles.OfType<DataItemVehicle>());
            dataChangedEvent.Fuel.NewItems.AddRange(newFuel.OfType<DataItemFuelEntry>());
            dataChangedEvent.MaintenanceRecords.NewItems.AddRange(newMaintenanceRecords.OfType<DataItemMaintenanceRecordEntry>());
            dataChangedEvent.MaintenanceSchedule.NewItems.AddRange(newMaintenanceSchedule.OfType<DataItemMaintenanceScheduleItem>());

            dataChangedEvent.Vehicles.EditedItems.AddRange(existingVehicles.OfType<DataItemVehicle>());
            dataChangedEvent.Fuel.EditedItems.AddRange(existingFuel.OfType<DataItemFuelEntry>());
            dataChangedEvent.MaintenanceRecords.EditedItems.AddRange(existingMaintenanceRecords.OfType<DataItemMaintenanceRecordEntry>());
            dataChangedEvent.MaintenanceSchedule.EditedItems.AddRange(existingMaintenanceSchedule.OfType<DataItemMaintenanceScheduleItem>());

            try
            {
                // Existing items are already tracked by EF Core from GetExistingItems,
                // so changes will be detected and saved automatically

                // Add the new items
                timeTracker = TimeTracker.Start();
                var allNew = newVehicles
                    .Concat(newFuel)
                    .Concat(newMaintenanceRecords)
                    .Concat(newMaintenanceSchedule);
                if (allNew.Any())
                    AddItemsToDbSets(allNew);
                timeTracker.End(3, $"CommitChanges InsertAll");

                // And delete the deleted items
                timeTracker = TimeTracker.Start();
                var vehiclesToDelete = dataChanges.Vehicles.IdentifiersToDelete.ToArray();
                if (vehiclesToDelete.Length > 0)
                {
                    DeleteVehicles(vehiclesToDelete);
                    dataChangedEvent.Vehicles.DeletedItems.DeletedIdentifiers.AddRange(vehiclesToDelete);
                }
                var fuelToDelete = dataChanges.Fuel.IdentifiersToDelete.ToArray();
                if (fuelToDelete.Length > 0)
                {
                    DeleteFuel(fuelToDelete);
                    dataChangedEvent.Fuel.DeletedItems.DeletedIdentifiers.AddRange(fuelToDelete);
                }
                var recordsToDelete = dataChanges.MaintenanceRecords.IdentifiersToDelete.ToArray();
                if (recordsToDelete.Length > 0)
                {
                    DeleteMaintenanceRecords(recordsToDelete);
                    dataChangedEvent.MaintenanceRecords.DeletedItems.DeletedIdentifiers.AddRange(recordsToDelete);
                }
                var schedulesToDelete = dataChanges.MaintenanceSchedule.IdentifiersToDelete.ToArray();
                if (schedulesToDelete.Length > 0)
                {
                    DeleteMaintenanceSchedule(schedulesToDelete);
                    dataChangedEvent.MaintenanceSchedule.DeletedItems.DeletedIdentifiers.AddRange(schedulesToDelete);
                }
                timeTracker.End(3, $"CommitChanges Delete");

                // Save the properties that have been changed so we can upload them later
                if (changedItems != null)
                    await changedItems.Save();

                _db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

            // Queue the Appointments to be updated (this saves the account so that it's flagged as Appointments not updated, and then does remaining work on separate thread)
            bool needsSave = false;

            // Only update appointments if we're not in a multi-part sync
            if (processType != ProcessType.OnlineMultiPart)
            {
            }

            // send out changed event
            if (DataChangedEvent != null)
            {
                timeTracker = TimeTracker.Start();

                // Don't let errors in UI break our data code
                try
                {
                    DataChangedEvent(this, dataChangedEvent);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
                }

                timeTracker.End(3, "CommitChanges sending out DataChangedEvent");
            }

            NotifyDataChangedEventHandlers(dataChangedEvent);

            return new CommitChangesResponse()
            {
                NeedsAccountToBeSaved = needsSave
            };

            // TODO - update tiles and toasts (should be done on a separate thread since not data-lock critical, but if another change comes in we should cancel the previous thread and simply complete the update on the new thread)
        }

        private void ImportEditedItems<T>(
            DataChanges.ScopedDataChanges<T> scopedDataChanges,
            ProcessType processType,
            ref DateTime now,
            List<BaseDataItem> existingDataItems,
            List<BaseDataItem> newDataItems,
            ChangedItems.ScopedChangedItems scopedChanges) where T : BaseDataItem, new()
        {
            foreach (BaseDataItem edited in scopedDataChanges.EditedItems)
            {
                // Assign the Updated time if this is local changes
                if (processType == ProcessType.Local)
                    edited.Updated = now;

                // Find the existing item, if there is one
                BaseDataItem existing = existingDataItems.FirstOrDefault(i => i.Identifier == edited.Identifier);

                // If there wasn't an existing data item
                if (existing == null)
                {
                    // We let this edited item become the item to save
                    BaseDataItem newItem = edited;

                    // And we also assign the DateCreated if this is local
                    if (processType == ProcessType.Local)
                        newItem.DateCreated = now;

                    // Flag that it's a new item
                    if (scopedChanges != null)
                        scopedChanges.AddNewItem(newItem.Identifier);

                    // Add it to our collection of new items to save
                    newDataItems.Add(newItem);
                }

                // Otherwise we need to copy properties into the existing
                else
                {
                    // Assign account to existing
                    existing.Account = Account;

                    // Apply the changes, while also getting which properties were actually affected
                    var changedPropertyNames = existing.ImportChanges(edited);

                    // And then flag that item as edited
                    if (scopedChanges != null)
                        scopedChanges.AddEditedItem(existing.Identifier, changedPropertyNames);
                }

                // Increment the now time so that each one is unique and maintains order
                now = now.AddTicks(1);
            }
        }

        private void DeleteVehicles(Guid[] identifiersToDelete)
        {
            var toDelete = ActualTableVehicles.Where(i => identifiersToDelete.Contains(i.Identifier)).ToArray();
            if (toDelete.Length > 0)
            {
                _db.Vehicles.RemoveRange(toDelete);

                // Delete children
                var toDeleteFuel = ActualTableFuel.Where(i => identifiersToDelete.Contains(i.VehicleIdentifier)).ToArray();
                if (toDeleteFuel.Length > 0)
                {
                    _db.FuelEntries.RemoveRange(toDeleteFuel);
                }

                var toDeleteSchedules = ActualTableMaintenanceSchedule.Where(i => identifiersToDelete.Contains(i.VehicleIdentifier)).ToArray();
                if (toDeleteSchedules.Length > 0)
                {
                    _db.MaintenanceScheduleItems.RemoveRange(toDeleteSchedules);
                }

                var toDeleteRecords = ActualTableMaintenanceRecords.Where(i => identifiersToDelete.Contains(i.VehicleIdentifier)).ToArray();
                if (toDeleteRecords.Length > 0)
                {
                    _db.MaintenanceRecordEntries.RemoveRange(toDeleteRecords);
                }
            }
        }

        private void DeleteFuel(Guid[] identifiersToDelete)
        {
            var toDelete = ActualTableFuel.Where(i => identifiersToDelete.Contains(i.Identifier)).ToArray();
            if (toDelete.Length > 0)
            {
                _db.FuelEntries.RemoveRange(toDelete);
            }
        }

        private void DeleteMaintenanceRecords(Guid[] identifiersToDelete)
        {
            var toDelete = ActualTableMaintenanceRecords.Where(i => identifiersToDelete.Contains(i.Identifier)).ToArray();
            if (toDelete.Length > 0)
            {
                _db.MaintenanceRecordEntries.RemoveRange(toDelete);
            }
        }

        private void DeleteMaintenanceSchedule(Guid[] identifiersToDelete)
        {
            var toDelete = ActualTableMaintenanceSchedule.Where(i => identifiersToDelete.Contains(i.Identifier)).ToArray();
            if (toDelete.Length > 0)
            {
                _db.MaintenanceScheduleItems.RemoveRange(toDelete);
            }
        }

        /// <summary>
        /// Doesn't enumerate all tables unless necessary. Enumerates item by item so all of them aren't loaded at once.
        /// </summary>
        /// <param name="identifiersToLookFor"></param>
        /// <returns></returns>
        private IEnumerable<BaseDataItem> FindAll<S>(Guid[] identifiersToLookFor)
        {
            var syncType = typeof(S);

            if (syncType == typeof(SyncItemVehicle))
            {
                return FindAll(identifiersToLookFor, ActualTableVehicles);
            }

            else if (syncType == typeof(SyncItemFuelEntry))
            {
                return FindAll(identifiersToLookFor, ActualTableFuel);
            }

            else if (syncType == typeof(SyncItemMaintenanceRecordEntry))
            {
                return FindAll(identifiersToLookFor, ActualTableMaintenanceRecords);
            }

            else if (syncType == typeof(SyncItemMaintenanceScheduleItem))
            {
                return FindAll(identifiersToLookFor, ActualTableMaintenanceSchedule);
            }

            else
            {
                throw new NotImplementedException("Unknown type " + syncType);
            }
        }

        private IEnumerable<BaseDataItem> FindAll<T>(Guid[] identifiersToLookFor, IQueryable<T> table) where T : BaseDataItem
        {
            return table.Where(i => identifiersToLookFor.Contains(i.Identifier));
        }

        private List<BaseDataItem> GetExistingItems<T>(IEnumerable<BaseDataItem> itemsToMatch) where T : BaseDataItem, new()
        {
            List<BaseDataItem> existingItems = new List<BaseDataItem>();

            // Only take 500 at once since SQL can only have max of 999 parameters (and might as well stay further below that limit)
            foreach (Guid[] identifiersBatchGroup in itemsToMatch.OfType<T>().Select(i => i.Identifier).BatchAsArrays(500))
            {
                if (identifiersBatchGroup.Length == 0)
                    break;

                if (typeof(T) == typeof(DataItemVehicle))
                {
                    existingItems.AddRange(_db.Vehicles.Where(i => identifiersBatchGroup.Contains(i.Identifier)));
                }
                else if (typeof(T) == typeof(DataItemFuelEntry))
                {
                    existingItems.AddRange(_db.FuelEntries.Where(i => identifiersBatchGroup.Contains(i.Identifier)));
                }
                else if (typeof(T) == typeof(DataItemMaintenanceRecordEntry))
                {
                    existingItems.AddRange(_db.MaintenanceRecordEntries.Where(i => identifiersBatchGroup.Contains(i.Identifier)));
                }
                else if (typeof(T) == typeof(DataItemMaintenanceScheduleItem))
                {
                    existingItems.AddRange(_db.MaintenanceScheduleItems.Where(i => identifiersBatchGroup.Contains(i.Identifier)));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return existingItems;
        }
    }
}
