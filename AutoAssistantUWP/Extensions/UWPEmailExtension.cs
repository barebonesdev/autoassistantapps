using AutoAssistantAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace AutoAssistantUWP.Extensions
{
    public class UWPEmailExtension : EmailExtension
    {
        public override async Task ComposeNewMailAsync(string to, string subject)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri($"mailto:?to={to}&subject={subject}"));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
