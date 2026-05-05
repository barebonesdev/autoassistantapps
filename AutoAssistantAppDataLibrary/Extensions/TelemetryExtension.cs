using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Extensions
{
    public abstract class TelemetryExtension
    {
        public static TelemetryExtension Current { get; set; }

        public abstract void TrackException(Exception ex, SeverityLevel severityLevel = SeverityLevel.Critical, ExceptionHandledAt handledAt = ExceptionHandledAt.UserCode);

        public abstract void TrackEvent(string eventName);

        public abstract void UpdateCurrentUser(AccountDataItem account);
    }

    namespace Telemetry
    {
        public enum SeverityLevel
        {
            //
            // Summary:
            //     Verbose severity level.
            Verbose = 0,
            //
            // Summary:
            //     Information severity level.
            Information = 1,
            //
            // Summary:
            //     Warning severity level.
            Warning = 2,
            //
            // Summary:
            //     Error severity level.
            Error = 3,
            //
            // Summary:
            //     Critical severity level.
            Critical = 4
        }

        public enum ExceptionHandledAt
        {
            //
            // Summary:
            //     Exception was not handled. Application crashed.
            Unhandled = 0,
            //
            // Summary:
            //     Exception was handled in user code.
            UserCode = 1,
            //
            // Summary:
            //     Exception was handled by some platform handlers.
            Platform = 2
        }
    }
}
