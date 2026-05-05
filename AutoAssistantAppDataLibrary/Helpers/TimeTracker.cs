using AutoAssistantAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Helpers
{
    public class TimeTracker
    {
        private DateTime StartTime { get; set; } = DateTime.UtcNow;

        private TimeTracker()
        {

        }

        public static TimeTracker Start()
        {
            return new TimeTracker();
        }

        public void End(double secondsTooLong, Func<string> actionGenerateMessage)
        {
            double totalSeconds = (DateTime.UtcNow - StartTime).TotalSeconds;
            if (totalSeconds >= secondsTooLong)
            {
                string message = $"Operation took too long ({totalSeconds.ToString("0")} seconds). Message: ";

                message += actionGenerateMessage();

                TelemetryExtension.Current?.TrackException(new Exception(message));
            }
        }

        public void End(double secondsTooLong, string message)
        {
            End(secondsTooLong, delegate { return message; });
        }
    }
}
