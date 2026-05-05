using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsPortable.Locks;

namespace AutoAssistantAppDataLibraryTests.FakeData
{
    public class FakeDispatcher : PortableDispatcher
    {
        private static MyAsyncLock _lock = new MyAsyncLock();

        public override async Task RunAsync(Action codeToExecute)
        {
            await Task.Run(async delegate
            {
                using (await _lock.LockAsync())
                {
                    codeToExecute();
                }
            });
        }
    }
}
