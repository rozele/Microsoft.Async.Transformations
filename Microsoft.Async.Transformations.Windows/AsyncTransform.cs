using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

using static Microsoft.Async.Transformations.AsyncTransform;

namespace Microsoft.Async.Transformations.Windows
{
    public static class AsyncTransform
    {
        public static Func<CancellationToken, Task> OnDispatcher(Func<CancellationToken, Task> asyncFunc)
        {
            return OnDispatcher(asyncFunc, CoreDispatcherPriority.Normal);
        }

        public static Func<CancellationToken, Task> OnDispatcher(Func<CancellationToken, Task> asyncFunc, CoreDispatcherPriority priority)
        {
            var dispatcher = Window.Current?.Dispatcher;
            if (dispatcher == null)
            {
                throw new InvalidOperationException("Could not access current window.");
            }

            return async token =>
            {
                if (dispatcher.HasThreadAccess)
                {
                    await asyncFunc(token);
                }
                else
                {
                    var taskCompletionSource = new TaskCompletionSource<bool>();
                    await dispatcher.RunAsync(priority, async () =>
                    {
                        try
                        {
                            await asyncFunc(token);
                            taskCompletionSource.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            taskCompletionSource.SetException(ex);
                        }
                    });

                    await taskCompletionSource.Task.ConfigureAwait(false);
                }
            };
        }

        public static DisposableAsyncFunction OnDispatcher(DisposableAsyncFunction asyncFunc)
        {
            return OnDispatcher(asyncFunc, CoreDispatcherPriority.Normal);
        }

        public static DisposableAsyncFunction OnDispatcher(DisposableAsyncFunction asyncFunc, CoreDispatcherPriority priority)
        {
            return new DisposableAsyncFunction(OnDispatcher(Identity(asyncFunc.InvokeAsync), priority), asyncFunc);
        }
    }
}
