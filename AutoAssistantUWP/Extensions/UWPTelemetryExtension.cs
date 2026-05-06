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
            Helpers.TelemetryHelper.TrackException(ex);
        }

        public override void TrackEvent(string eventName)
        {
            Helpers.TelemetryHelper.TrackEvent(eventName);
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
