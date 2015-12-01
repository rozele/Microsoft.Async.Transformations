using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Microsoft.Async.Transformations.Windows
{
    public static class AsyncTransformExtensions
    {
        public static Func<CancellationToken, Task> OnDispatcher(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.OnDispatcher(asyncFunc);
        }

        public static Func<CancellationToken, Task> OnDispatcher(this Func<CancellationToken, Task> asyncFunc, CoreDispatcherPriority priority)
        {
            return AsyncTransform.OnDispatcher(asyncFunc, priority);
        }

        public static DisposableAsyncFunction OnDispatcher(this DisposableAsyncFunction asyncFunc)
        {
            return AsyncTransform.OnDispatcher(asyncFunc);
        }

        public static DisposableAsyncFunction OnDispatcher(this DisposableAsyncFunction asyncFunc, CoreDispatcherPriority priority)
        {
            return AsyncTransform.OnDispatcher(asyncFunc, priority);
        }
    }
}
