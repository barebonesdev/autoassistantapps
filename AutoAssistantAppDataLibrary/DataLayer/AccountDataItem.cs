using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using AutoAssistantLibrary.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    [DataContract(Namespace = "")]
    public class AccountDataItem : BindableBaseWithPortableDispatcher
    {
        public const int CURRENT_ACCOUNT_DATA_VERSION = 1;
        public const int CURRENT_SYNCED_DATA_VERSION = 1;

        /// <summary>
        /// Initializes a new account
        /// </summary>
        /// <param name="localAccountId"></param>
        public AccountDataItem(Guid localAccountId)
        {
            LocalAccountId = localAccountId;
        }


        public Guid LocalAccountId { get; internal set; }

        [DataMember]
        public Version Version = Variables.VERSION;

        [DataMember]
        public int AccountDataVersion = CURRENT_ACCOUNT_DATA_VERSION;

        /// <summary>
        /// SyncedDataVersion is used so that when we make drastic changes, we can perform needed actions upon upgrade.
        /// For example, when we added grades linked to homework/exams, we need down-level clients to re-sync
        /// all homework/exams, so that they pick up the new grade values.
        /// </summary>
        [DataMember]
        public int SyncedDataVersion { get; set; } = CURRENT_SYNCED_DATA_VERSION;

        private int _currentChangeNumberVehicles;
        [DataMember]
        public int CurrentChangeNumberVehicles
        {
            get { return _currentChangeNumberVehicles; }
            set { SetProperty(ref _currentChangeNumberVehicles, value, nameof(CurrentChangeNumberVehicles)); }
        }

        private int _currentChangeNumberFuel;
        [DataMember]
        public int CurrentChangeNumberFuel
        {
            get { return _currentChangeNumberFuel; }
            set { SetProperty(ref _currentChangeNumberFuel, value, nameof(CurrentChangeNumberFuel)); }
        }

        private int _currentChangeNumberMaintenanceRecords;
        [DataMember]
        public int CurrentChangeNumberMaintenanceRecords
        {
            get { return _currentChangeNumberMaintenanceRecords; }
            set { SetProperty(ref _currentChangeNumberMaintenanceRecords, value, nameof(CurrentChangeNumberMaintenanceRecords)); }
        }

        private int _currentChangeNumberMaintenanceSchedule;
        [DataMember]
        public int CurrentChangeNumberMaintenanceSchedule
        {
            get { return _currentChangeNumberMaintenanceSchedule; }
            set { SetProperty(ref _currentChangeNumberMaintenanceSchedule, value, nameof(CurrentChangeNumberMaintenanceSchedule)); }
        }

        #region Settings

        private bool _needsToSyncSettings;
        /// <summary>
        /// In case user exits or is offline when they modify settings, I'll remember that I need to sync them
        /// </summary>
        [DataMember]
        public bool NeedsToSyncSettings
        {
            get { return _needsToSyncSettings; }
            set { SetProperty(ref _needsToSyncSettings, value, "NeedsToSyncSettings"); }
        }

        private async System.Threading.Tasks.Task SaveOnThread()
        {
            await AccountsManager.Save(this);
        }

        private bool _reviewed;
        /// <summary>
        /// If the user hasn't reviewed the app, this should be false.
        /// </summary>
        [DataMember]
        public bool Reviewed
        {
            get { return _reviewed; }
            set { SetProperty(ref _reviewed, value, "Reviewed"); }
        }

        private bool _isPushDisabled = false;
        [DataMember]
        public bool IsPushDisabled
        {
            get { return _isPushDisabled; }
            set { SetProperty(ref _isPushDisabled, value, "IsPushDisabled"); }
        }

        #endregion


        private int _deviceId;
        [DataMember]
        public int DeviceId
        {
            get { return _deviceId; }
            set { SetProperties(ref _deviceId, value, "DeviceId", "IsOnlineAccount"); }
        }

        private long _accountId;
        [DataMember]
        public long AccountId
        {
            get { return _accountId; }
            set { SetProperties(ref _accountId, value, "AccountId", "IsOnlineAccount"); }
        }

        private string _username;
        [DataMember]
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, "Username"); }
        }

        private string _password;
        [DataMember]
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, "Password"); }
        }

        private bool _rememberUsername;
        [DataMember]
        public bool RememberUsername
        {
            get { return _rememberUsername; }
            set
            {
                SetProperties(ref _rememberUsername, value, "RememberUsername", "IsRememberPasswordPossible", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _rememberPassword;
        [DataMember]
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                SetProperties(ref _rememberPassword, value, "RememberPassword", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _autoLogin;
        [DataMember]
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set { SetProperty(ref _autoLogin, value, "AutoLogin"); }
        }

        public bool IsRememberPasswordPossible
        {
            get { return RememberUsername; }
        }

        public bool IsAutoLoginPossible
        {
            get { return RememberUsername && RememberPassword; }
        }

        public bool IsOnlineAccount
        {
            get { return AccountId != 0 && DeviceId != 0; }
        }

        public LoginCredentials GenerateCredentials()
        {
            return new LoginCredentials()
            {
                Username = Username,
                Session = Password,
                AccountId = AccountId
            };
        }


        /// <summary>
        /// Submits changes.
        /// </summary>
        public async System.Threading.Tasks.Task ConvertToLocal()
        {
            AccountId = 0;
            DeviceId = 0;

            await SaveOnThread();

            //mark all items changed
            //Changes.ClearAll();

            //clear all changes
            throw new NotImplementedException();

            //foreach (BaseItemWin item in School.GetAllChildren())
            //    Changes.Add(item);

        }

        /// <summary>
        /// Places all the items into the changes storage so they'll be synced
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task ConvertToOnline()
        {
            throw new NotImplementedException();
            //partialChanges = new PartialChanges();

            //foreach (BaseItemWin item in School.GetAllChildren())
            //    partialChanges.New(item.Identifier);

            //await savePartialChanges();
        }

        private Guid _currentVehicleId;
        /// <summary>
        /// Stored and saved to data. Not guaranteed to be a valid vehicle ID (for example vehicle might have been deleted)
        /// </summary>
        [DataMember]
        public Guid CurrentVehicleId
        {
            get { return _currentVehicleId; }
            internal set { _currentVehicleId = value; }
        }

        /// <summary>
        /// Sets current vehicle and saves changes, also updates primary tile
        /// </summary>
        public async System.Threading.Tasks.Task SetCurrentVehicleAsync(Guid currentVehicleId, bool uploadSettings = true)
        {
            if (CurrentVehicleId == currentVehicleId)
                return;

            // If semester is being cleared (going to Years page), ignore this change.
            // That's to allow easily being able to view overall GPA without losing curr semester.
            if (currentVehicleId == Guid.Empty)
            {
                return;
            }

            CurrentVehicleId = currentVehicleId;

            NeedsToSyncSettings = true;
            await AccountsManager.Save(this);

            // Upload their changed setting
            if (uploadSettings)
            {
                //var dontWait = Sync.SyncSettings(this, Sync.ChangedSetting.SelectedSemesterId);
            }

            var dataStore = await AccountDataStore.Get(this.LocalAccountId);
        }

        /// <summary>
        /// Syncs account and updates tiles, without awaiting
        /// </summary>
        public async void ExecuteOnLoginTasks()
        {
            try
            {
                var dataStore = await AccountDataStore.Get(this.LocalAccountId);

                // MainScreenViewModel will start the sync

            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
            }
        }

        public DateTime LastSyncOn { get; internal set; }
    }
}
