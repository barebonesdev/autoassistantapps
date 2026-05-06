using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Extensions
{
    public abstract class BrowserExtension
    {
        public static BrowserExtension Current { get; set; }

        public abstract Task OpenUrlAsync(Uri uri);
    }
}
