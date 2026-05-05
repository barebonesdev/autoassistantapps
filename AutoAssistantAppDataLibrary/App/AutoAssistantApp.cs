using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using AutoAssistantAppDataLibrary.SyncLayer;
using AutoAssistantAppDataLibrary.Windows;
using BareMvvm.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.App
{
    public class AutoAssistantApp : PortableApp
    {
        public new static AutoAssistantApp Current
        {
            get { return PortableApp.Current as AutoAssistantApp; }
        }

        public AccountDataItem GetCurrentAccount()
        {
            return (GetCurrentWindow() as MainAppWindow)?.GetCurrentAccount();
        }

        public async Task SaveChanges(DataChanges changes)
        {
            var account = GetCurrentAccount();

            if (account == null)
            {
                throw new NullReferenceException("account was null. Windows: " + Windows.Count);
            }

            var dataStore = await AccountDataStore.Get(account.LocalAccountId);
            await dataStore.ProcessLocalChanges(changes);

            // Don't await this, we don't want it blocking
            if (account.IsOnlineAccount)
            {
                SyncWithoutBlocking(account);
            }
        }

        private async void SyncWithoutBlocking(AccountDataItem account)
        {
            try
            {
                await Sync.SyncAccountAsync(account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, SeverityLevel.Error);
            }
        }
    }
}
