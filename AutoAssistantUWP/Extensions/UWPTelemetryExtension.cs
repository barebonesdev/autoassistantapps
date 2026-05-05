using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantUWP.Extensions
{
    public class UWPTelemetryExtension : TelemetryExtension
    {
        public override void TrackException(Exception ex, SeverityLevel severityLevel = SeverityLevel.Critical, ExceptionHandledAt handledAt = AutoAssistantAppDataLibrary.Extensions.Telemetry.ExceptionHandledAt.UserCode)
        {
            Helpers.TelemetryHelper.TrackException(ex, GetSeverityLevel(severityLevel), GetHandledAt(handledAt));
        }

        public override void TrackEvent(string eventName)
        {
            Helpers.TelemetryHelper.TrackEvent(eventName);
        }

        private Microsoft.HockeyApp.SeverityLevel GetSeverityLevel(SeverityLevel level)
        {
            switch (level)
            {
                case SeverityLevel.Critical:
                    return Microsoft.HockeyApp.SeverityLevel.Critical;

                case SeverityLevel.Error:
                    return Microsoft.HockeyApp.SeverityLevel.Error;

                case SeverityLevel.Information:
                    return Microsoft.HockeyApp.SeverityLevel.Information;

                case SeverityLevel.Verbose:
                    return Microsoft.HockeyApp.SeverityLevel.Verbose;

                case SeverityLevel.Warning:
                    return Microsoft.HockeyApp.SeverityLevel.Warning;

                default:
                    return Microsoft.HockeyApp.SeverityLevel.Critical;
            }
        }

        private Helpers.ExceptionHandledAt GetHandledAt(ExceptionHandledAt handledAt)
        {
            switch (handledAt)
            {
                case AutoAssistantAppDataLibrary.Extensions.Telemetry.ExceptionHandledAt.Platform:
                    return Helpers.ExceptionHandledAt.Unhandled;

                case AutoAssistantAppDataLibrary.Extensions.Telemetry.ExceptionHandledAt.UserCode:
                    return Helpers.ExceptionHandledAt.UserCode;

                case AutoAssistantAppDataLibrary.Extensions.Telemetry.ExceptionHandledAt.Unhandled:
                    return Helpers.ExceptionHandledAt.Unhandled;

                default:
                    return Helpers.ExceptionHandledAt.Unhandled;
            }
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            try
            {
                Helpers.TelemetryHelper.UpdateCurrentUser(account);
            }
            catch { }
        }
    }
}
