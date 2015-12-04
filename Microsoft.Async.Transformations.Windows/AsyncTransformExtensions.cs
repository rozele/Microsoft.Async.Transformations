using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Microsoft.Async.Transformations.Windows
{
    /// <summary>
    /// A set of extensions for asynchronous functions.
    /// </summary>
    public static class AsyncTransformExtensions
    {
        /// <summary>
        /// Converts a asynchronous function into one that is guaranteed to be 
        /// called on a dispatcher thread.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <returns>The transformed asynchronous function.</returns>
        public static Func<CancellationToken, Task> OnDispatcher(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.OnDispatcher(asyncFunc);
        }

        /// <summary>
        /// Converts a asynchronous function into one that is guaranteed to be 
        /// called on a dispatcher thread.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="priority">The dispatcher priority.</param>
        /// <returns>The transformed asynchronous function.</returns>
        public static Func<CancellationToken, Task> OnDispatcher(this Func<CancellationToken, Task> asyncFunc, CoreDispatcherPriority priority)
        {
            return AsyncTransform.OnDispatcher(asyncFunc, priority);
        }
    }
}
