using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.Extensions;
using AutoAssistantAppDataLibrary.Extensions.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantiOS.Extensions
{
    public class iOSTelemetryExtension : TelemetryExtension
    {
        public override void TrackException(Exception ex, SeverityLevel severityLevel = SeverityLevel.Critical, ExceptionHandledAt handledAt = AutoAssistantAppDataLibrary.Extensions.Telemetry.ExceptionHandledAt.UserCode)
        {
            Console.WriteLine(ex.ToString());
        }

        public override void TrackEvent(string eventName)
        {
            Console.WriteLine($"Event: {eventName}");
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
        }
    }
}