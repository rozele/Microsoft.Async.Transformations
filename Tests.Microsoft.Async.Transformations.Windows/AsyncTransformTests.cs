using Microsoft.Async.Transformations.Windows;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

using static Microsoft.Async.Transformations.AsyncTransform;

namespace Tests.Microsoft.Async.Transformations.Windows
{
    [TestClass]
    public class AsyncTransformTests
    {
        [TestMethod]
        public async Task AsyncTransform_OnDispatcher()
        {
            var dispatcher = App.Dispatcher;

            var task1 = Task.Run(() => CheckAccess(dispatcher));
            var task2 = Task.Run(() => CheckAccess(dispatcher));
            if (await task1 && await task2)
            {
                Assert.Inconclusive("Unexpected access.");
            }

            var asyncFunc = default(Func<CancellationToken, Task>);

            var mre = new ManualResetEvent(false);

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                asyncFunc = AsAsync(() => Assert.IsTrue(CheckAccess(dispatcher))).OnDispatcher();
                mre.Set();
            });

            mre.WaitOne();

            await asyncFunc(CancellationToken.None);
        }

        private bool CheckAccess(CoreDispatcher dispatcher)
        {
            return dispatcher.HasThreadAccess;
        }
    }
}
