using System;
using System.Threading.Tasks;
using AutoAssistantAppDataLibrary.Extensions;
using Foundation;
using UIKit;

namespace AutoAssistantiOS.Extensions
{
    public class iOSBrowserExtension : BrowserExtension
    {
        public override Task OpenUrlAsync(Uri uri)
        {
            return UIApplication.SharedApplication.OpenUrlAsync(new NSUrl(uri.ToString()), new UIApplicationOpenUrlOptions());
        }
    }
}
