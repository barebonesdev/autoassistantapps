using AutoAssistantAppDataLibrary.DataLayer;
using Microsoft.HockeyApp;
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

        /// <summary>
        /// Main process needs to initialize this so that symbols from everything will be loaded (since the client will be coming from the parent project now)
        /// </summary>
        public static IHockeyClient TelemetryClient;

        public static void TrackException(Exception ex, SeverityLevel severityLevel = SeverityLevel.Critical, ExceptionHandledAt handledAt = ExceptionHandledAt.UserCode)
        {
            try
            {
                if (TelemetryClient == null)
                    return;

                Dictionary<string, string> additional = new Dictionary<string, string>()
                {
                    { "HandledAt", handledAt.ToString() },
                    { "SeverityLevel", severityLevel.ToString() },
                    { "AccountId", _currentAccountId.ToString() }
                };

                TelemetryClient.TrackException(ex, additional);
            }

            catch { }
        }

        public static void TrackEvent(string eventName)
        {
            try
            {
                if (TelemetryClient == null)
                {
                    return;
                }

                TelemetryClient.TrackEvent(eventName);
                System.Diagnostics.Debug.WriteLine("Tracked event: " + eventName);

                // TrackPageView does NOT work with HockeyApp, nothing is viewable in the web client for that
                // TrackEvent works with both just the string and the object overload, but the only thing viewable in the
                // web client is the event name, the metrics, properties, etc aren't viewable

                //TelemetryClient.TrackEvent(new Microsoft.HockeyApp.DataContracts.EventTelemetry("DevelopmentEvent1")
                //{
                //    Metrics =
                //    {
                //        { "CustomMetric1", 4.4 }
                //    },
                //    Properties =
                //    {
                //        { "CustomProperty1", "Burritos" }
                //    },
                //    Sequence = "Home.DevelopmentEvent1"
                //});

            }
            catch { }
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
