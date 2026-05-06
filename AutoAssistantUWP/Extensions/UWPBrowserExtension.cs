using AutoAssistantAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace AutoAssistantUWP.Extensions
{
    public class UWPBrowserExtension : BrowserExtension
    {
        public override async Task OpenUrlAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
