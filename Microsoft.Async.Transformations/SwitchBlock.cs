using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Async.Transformations
{
    /// <summary>
    /// A command helper that ensures activities are mutually exclusive. The
    /// individual actions that are added can be triggered sequentially an
    /// infinite number of times, leaving the implementation of the action to
    /// handle things like restart or toggle behavior. However, if a sequence
    /// of actions is interrupted by a different action, the entire sequence is
    /// cancelled and the interrupting action will begin. 
    /// </summary>
    public sealed class SwitchBlock : IDisposable
    {
        private readonly object _gate = new object();
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private object _active;
        private CancellationDisposable _cancelSwitch;
        private Task _currentTask;

        /// <summary>
        /// Add an action to the switch.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The switched action.</returns>
        public Func<CancellationToken, Task> Add(Func<CancellationToken, Task> action)
        {
            var current = new object();
            return token => ExecuteAsync(current, action, token);
        }

        private async Task ExecuteAsync(object current, Func<CancellationToken, Task> action, CancellationToken token)
        {
            // Create a task completion source.
            var doneTaskSource = new TaskCompletionSource<bool>();

            // Set the "current task".
            var doneTask = doneTaskSource.Task;

            // Local variables for the switch state.
            var active = default(object);
            var currentTask = default(Task);
            var cancelSwitch = default(CancellationDisposable);

            // Enter the lock and update the switch state.
            lock (_gate)
            {
                // If nothing is active, the current call becomes active.
                if (_active == null)
                {
                    _active = active = current;
                    currentTask = _currentTask = doneTask;
                    _serialDisposable.Disposable = cancelSwitch = _cancelSwitch = new CancellationDisposable();
                }
                // If the current call is the active kind, repeat and add a continuation.
                else if (_active == current)
                {
                    active = _active;
                    cancelSwitch = _cancelSwitch;
                    currentTask = _currentTask = doneTask = _currentTask.ContinueWith(_ => doneTaskSource.Task).Unwrap();
                }
                // Otherwise, set the local variables for awaiting.
                else
                {
                    active = _active;
                    currentTask = _currentTask;
                    cancelSwitch = _cancelSwitch;
                }
            }

            // If the current task is not active, cancel the active task and try again. 
            if (active != current)
            {
                cancelSwitch.Dispose();
                await currentTask;
                await ExecuteAsync(current, action, token);
            }
            // Else, run the current task.
            else
            {
                using (var localCancellationSource = new CancellationDisposable())
                using (cancelSwitch.Token.Register(localCancellationSource.Dispose))
                using (token.Register(localCancellationSource.Dispose))
                {
                    try
                    {
                        await action(localCancellationSource.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != localCancellationSource.Token)
                        {
                            throw;
                        }

                        token.ThrowIfCancellationRequested();

                        if (_serialDisposable.IsDisposed)
                        {
                            throw new ObjectDisposedException("SwitchBlock");
                        }
                    }
                    finally
                    {
                        lock (_gate)
                        {
                            // If the current task is the final task, reset the active task.
                            if (_currentTask == doneTask)
                            {
                                _active = null;
                            }
                        }

                        doneTaskSource.SetResult(true);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes the switch block.
        /// </summary>
        public void Dispose()
        {
            _serialDisposable.Dispose();
        }
    }
}
