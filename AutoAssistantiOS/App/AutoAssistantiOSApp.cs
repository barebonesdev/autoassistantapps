using AutoAssistantAppDataLibrary.App;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoAssistantiOS.App
{
    public class AutoAssistantiOSApp : AutoAssistantApp
    {
        protected override Task InitializeAsyncOverride()
        {
            AutoAssistantAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Auto Assistant for iOS"; };

            // Note that there's several places my code takes a dependency on this to change behavior for iOS version
            AutoAssistantAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate { return "iOS"; };

            return base.InitializeAsyncOverride();
        }
    }
}
