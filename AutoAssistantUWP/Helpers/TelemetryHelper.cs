using AutoAssistantAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantUWP.Helpers
{
    public static class TelemetryHelper
    {
        public const string HOCKEY_APP_ID = "8137129f682c44518218fdcb5262403c";
        private static long _currentAccountId;

        public static void TrackException(Exception ex)
        {
        }

        public static void TrackEvent(string eventName)
        {
        }

        public static Action<AccountDataItem> UpdateCurrentUserExtension;

        public static void UpdateCurrentUser(AccountDataItem account)
        {
            if (account == null)
            {
                if (_currentAccountId == 0)
                    return;

                _currentAccountId = 0;
            }
            else
            {
                if (_currentAccountId == account.AccountId)
                    return;

                _currentAccountId = account.AccountId;
            }

            if (UpdateCurrentUserExtension != null)
            {
                try
                {
                    UpdateCurrentUserExtension(account);
                }

                catch { }
            }
        }
    }

    public enum ExceptionHandledAt
    {
        Unhandled,
        UserCode
    }
}
