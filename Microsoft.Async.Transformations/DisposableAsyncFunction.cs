using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Async.Transformations
{
    /// <summary>
    /// A wrapper for an async factory and disposable.
    /// </summary>
    public sealed class DisposableAsyncFunction : IDisposable
    {
        private readonly Func<CancellationToken, Task> _asyncFunction;
        private readonly IDisposable _disposable;

        /// <summary>
        /// Instantiates a <see cref="DisposableAsyncFunction"/>.
        /// </summary>
        /// <param name="asyncFunction">An asynchronous function.</param>
        /// <param name="disposable">A disposable.</param>
        public DisposableAsyncFunction(Func<CancellationToken, Task> asyncFunction, IDisposable disposable)
        {
            _asyncFunction = asyncFunction;
            _disposable = disposable;
        }

        /// <summary>
        /// Invokes the asynchronous function provided in the constructor.
        /// </summary>
        /// <param name="token">
        /// A token to cancel the asynchronous request.
        /// </param>
        /// <returns>A task to await the completion of the invocation.</returns>
        public Task InvokeAsync(CancellationToken token)
        {
            return _asyncFunction(token);
        }

        /// <summary>
        /// Disposes the <see cref="IDisposable"/> provided in the constructor.
        /// </summary>
        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
