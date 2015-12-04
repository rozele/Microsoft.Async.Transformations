using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

using static Microsoft.Async.Transformations.AsyncTransform;

namespace Microsoft.Async.Transformations.Windows
{
    /// <summary>
    /// A set of transformations for asynchronous functions specific to Windows.
    /// </summary>
    public static class AsyncTransform
    {
        /// <summary>
        /// Converts a asynchronous function into one that is guaranteed to be 
        /// called on a dispatcher thread.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <returns>The transformed asynchronous function.</returns>
        public static Func<CancellationToken, Task> OnDispatcher(Func<CancellationToken, Task> asyncFunc)
        {
            return OnDispatcher(asyncFunc, CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Converts a asynchronous function into one that is guaranteed to be 
        /// called on a dispatcher thread.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="priority">The dispatcher priority.</param>
        /// <returns>The transformed asynchronous function.</returns>
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
    }
}
