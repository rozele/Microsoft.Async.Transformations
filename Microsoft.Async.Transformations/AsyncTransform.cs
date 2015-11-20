using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Async.Transformations
{
    /// <summary>
    /// A set of transformations for asynchronous functions.
    /// </summary>
    public static class AsyncTransform
    {
        #region AsAsync

        /// <summary>
        /// Wraps an action as a asynchronous function.
        /// </summary>
        /// <param name="action">An action.</param>
        /// <returns>An asynchronous function wrapper around the action.</returns>
        /// <remarks>The cancellation token is ignored.</remarks>
        public static Func<CancellationToken, Task> AsAsync(Action action)
        {
            Requires(action, nameof(action));

            return _ =>
            {
                action();
                return Task.FromResult(true);
            };
        }

        #endregion

        #region AsCancellable

        /// <summary>
        /// Wraps an asynchronous function without cancellation as one with a
        /// cancellation token.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <returns>A cancellable wrapper around the asynchronous function.</returns>
        public static Func<CancellationToken, Task> AsCancellable(Func<Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            
            return Uncurry<CancellationToken, Task>(asyncFunc);
        }

        #endregion

        #region Condition

        /// <summary>
        /// Conditionally invokes an asynchronous function.
        /// </summary>
        /// <param name="if">An asynchronous function to invoke.</param>
        /// <param name="predicate">
        /// A function that returns the conditional flag.
        /// </param>
        /// <returns>
        /// An asynchronous function that is invoked only when the conditional
        /// function returns true.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// async token => 
        /// {
        ///     if (condition())
        ///         await @if(token);
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Condition(Func<CancellationToken, Task> @if, Func<bool> predicate)
        {
            Requires(@if, nameof(@if));
            Requires(predicate, nameof(predicate));

            return Condition(@if, predicate, Empty());
        }

        /// <summary>
        /// Conditionally invokes an asynchronous function.
        /// </summary>
        /// <param name="if">
        /// An asynchronous function to invoke if the condition is true.
        /// </param>
        /// <param name="predicate">
        /// A function that returns the conditional flag.
        /// </param>
        /// <param name="else">
        /// An asynchronous function to invoke if the condition is not true.
        /// </param>
        /// <returns>
        /// An asynchronous function that switches over the input asynchronous
        /// functions based on the result of the conditional function.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// async token => 
        /// {
        ///     if (condition())
        ///         await @if(token);
        ///     else
        ///         await @else(token);
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Condition(Func<CancellationToken, Task> @if, Func<bool> predicate, Func<CancellationToken, Task> @else)
        {
            Requires(@if, nameof(@if));
            Requires(predicate, nameof(predicate));
            Requires(@else, nameof(@else));

            return Curry(
                Condition(
                    Uncurry<Unit>(@if), 
                    Uncurry<Unit, bool>(predicate), 
                    Uncurry<Unit>(@else)),
                default(Unit));
        }

        /// <summary>
        /// Conditionally invokes an asynchronous function.
        /// </summary>
        /// <typeparam name="T">
        /// Type of state input into the conditional function.
        /// </typeparam>
        /// <param name="if">An asynchronous function to invoke.</param>
        /// <param name="predicate">
        /// A function that returns the conditional flag.
        /// </param>
        /// <returns>
        /// An asynchronous function that is invoked only when the conditional
        /// function returns true.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// async (input, token) => 
        /// {
        ///     if (condition(input))
        ///         await @if(token);
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<T, CancellationToken, Task> Condition<T>(Func<T, CancellationToken, Task> @if, Func<T, bool> predicate)
        {
            Requires(@if, nameof(@if));
            Requires(predicate, nameof(predicate));

            return Condition(@if, predicate, Empty<T>());
        }

        /// <summary>
        /// Conditionally invokes an asynchronous function.
        /// </summary>
        /// <typeparam name="T">
        /// Type of state input into the conditional function.
        /// </typeparam>
        /// <param name="if">
        /// An asynchronous function to invoke if the condition is true.
        /// </param>
        /// <param name="predicate">
        /// A function that returns the conditional flag.
        /// </param>
        /// <param name="else">
        /// An asynchronous function to invoke if the condition is not true.
        /// </param>
        /// <returns>
        /// An asynchronous function that switches over the input asynchronous
        /// functions based on the result of the conditional function.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// async (input, token) => 
        /// {
        ///     if (condition(input))
        ///         await @if(token);
        ///     else
        ///         await @else(token);
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<T, CancellationToken, Task> Condition<T>(Func<T, CancellationToken, Task> @if, Func<T, bool> predicate, Func<T, CancellationToken, Task> @else)
        {
            Requires(@if, nameof(@if));
            Requires(predicate, nameof(predicate));
            Requires(@else, nameof(@else));

            return (input, token) => predicate(input) ? @if(input, token) : @else(input, token);
        }

        #endregion

        #region Count

        /// <summary>
        /// Converts an asynchronous function with a <see cref="int"/> input
        /// into one that keeps track of the invocation count.
        /// </summary>
        /// <param name="asyncFunc">
        /// An asynchronous function that takes an invocation count as a parameter.
        /// </param>
        /// <returns>
        /// An asynchronous function that keeps track of the invocation count.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int count = 0;
        /// 
        /// async token =>
        /// {
        ///     await asyncFunc(++count, token);
        /// }
        /// </code>
        /// 
        /// Obviously, with consideration for concurrent increment.
        /// </remarks>
        public static Func<CancellationToken, Task> Count(Func<int, CancellationToken, Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            var index = 0;
            return token => asyncFunc(Interlocked.Increment(ref index), token);
        }

        #endregion

        #region Curry

        /// <summary>
        /// Curry an asynchronous function with a parameter to one without.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="arg">The argument to curry.</param>
        /// <returns>The curried asynchronous function.</returns>
        public static Func<CancellationToken, Task> Curry<T>(Func<T, CancellationToken, Task> asyncFunc, T arg)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            return token => asyncFunc(arg, token);
        }

        #endregion

        #region Empty

        private static readonly Func<CancellationToken, Task> s_empty = _ => Task.FromResult(true);

        /// <summary>
        /// An asynchronous function with no effects.
        /// </summary>
        /// <returns>The empty asynchronous function.</returns>
        /// <remarks>A singleton instance is used here.</remarks>
        public static Func<CancellationToken, Task> Empty()
        {
            return s_empty;
        }

        /// <summary>
        /// An asynchronous function with no effects.
        /// </summary>
        /// <typeparam name="T">Type of parameter.</typeparam>
        /// <returns>The empty asynchronous function.</returns>
        /// <remarks>A singleton instance is used here.</remarks>
        public static Func<T, CancellationToken, Task> Empty<T>()
        {
            return EmptyAsyncFunc<T>.Instance;
        }

        static class EmptyAsyncFunc<T>
        {
            private static readonly Func<T, CancellationToken, Task> s_instance = (_, __) => Task.FromResult(true);

            public static Func<T, CancellationToken, Task> Instance => s_instance; 
        }

        #endregion

        #region Exclusive

        /// <summary>
        /// Converts an asynchronous function into one that can only be accessed one at a time.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>An asynchronous function that only allows exclusive access.</returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// object gate = new object();
        /// 
        /// async token =>
        /// {
        ///     if (Monitor.TryEnter(gate))
        ///     {
        ///         try
        ///         {
        ///             await asyncFunc(token);
        ///         }
        ///         finally
        ///         {
        ///             Monitor.Exit(gate);
        ///         }
        ///     }
        /// }
        /// </code>
        /// 
        /// Note that non-exclusive requests are no-ops.
        /// </remarks>
        public static Func<CancellationToken, Task> Exclusive(Func<CancellationToken, Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            return Exclusive(asyncFunc, Empty());
        }

        /// <summary>
        /// Converts an asynchronous function into one that can only be accessed one at a time.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <param name="rest">
        /// An asynchronous function to invoke for non-exclusive requests.
        /// </param>
        /// <returns>An asynchronous function that only allows exclusive access.</returns>
        /// <remarks>
        /// The resulting asynchronous function has the same semantics as:
        /// 
        /// <code>
        /// object gate = new object();
        /// 
        /// async token =>
        /// {
        ///     if (Monitor.TryEnter(gate))
        ///     {
        ///         try
        ///         {
        ///             await asyncFunc(token);
        ///         }
        ///         finally
        ///         {
        ///             Monitor.Exit(gate);
        ///         }
        ///     }
        ///     else
        ///     {
        ///         await rest(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// Where the gate does not support re-entrancy.
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Exclusive(Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> rest)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(rest, nameof(rest));

            var acquired = 0;
            return Condition(
                Finally(
                    asyncFunc,
                    AsAsync(() => acquired = 0)), 
                () => Interlocked.CompareExchange(ref acquired, 1, 0) == 0, 
                rest);
        }

        #endregion

        #region Finally

        /// <summary>
        /// Chains an asynchronous function with an asynchronous function to 
        /// invoke regardless of the outcome.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="finallyAction">
        /// The asynchronous function to invoke regardless of outcome.
        /// </param>
        /// <returns>An asynchronous function combined with a finally block.</returns>
        /// <remarks>
        /// The resulting asynchronous function has the same semantics as:
        /// 
        /// <code>
        /// async token =>
        /// {
        ///     try
        ///     {
        ///         await asyncFunc(token);
        ///     }
        ///     finally
        ///     {
        ///         await @finally(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Finally(Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> finallyAction)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(finallyAction, nameof(finallyAction));

            return async token =>
            {
                try
                {
                    await asyncFunc(token).ConfigureAwait(false);
                }
                finally
                {
                    await finallyAction(token).ConfigureAwait(false);
                }
            };
        }

        #endregion

        #region First

        /// <summary>
        /// Converts an asynchronous function into one that is only triggered 
        /// the first time it is invoked.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>A transformed asynchronous function.</returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int count = 0;
        /// 
        /// async token =>
        /// {
        ///     if (count++ == 0)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> First(Func<CancellationToken, Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            return First(asyncFunc, Empty());
        }

        /// <summary>
        /// Converts an asynchronous function into one that is only triggered 
        /// the first time it is invoked.
        /// </summary>
        /// <param name="asyncFunc">
        /// An asynchronous function to use on the first invocation.
        /// </param>
        /// <param name="rest">
        /// An asynchronous function to use on all following invocations.
        /// </param>
        /// <returns>A transformed asynchronous function.</returns>
        /// <remarks>
        /// The resulting asynchronous function has the same semantics as:
        /// 
        /// <code>
        /// int count = 0;
        /// 
        /// async token =>
        /// {
        ///     if (count++ == 0)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        ///     else
        ///     {
        ///         await rest(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> First(Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> rest)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(rest, nameof(rest));

            return Take(asyncFunc, 1, rest);
        }

        #endregion

        #region Identity

        /// <summary>
        /// An identity function for asynchronous functions.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>The identity of the input.</returns>
        /// <remarks>
        /// This is particularly useful for syntactic sugar. E.g.,
        /// 
        /// <code>
        /// var asyncFunc = Identity(MyFunctionAsync);
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Identity(Func<CancellationToken, Task> asyncFunc)
        {
            return asyncFunc;
        }

        #endregion

        #region LongCount

        /// <summary>
        /// Converts an asynchronous function with a <see cref="long"/> input
        /// into one that keeps track of the invocation count.
        /// </summary>
        /// <param name="asyncFunc">
        /// An asynchronous function that takes an invocation count as a parameter.
        /// </param>
        /// <returns>
        /// An asynchronous function that keeps track of the invocation count.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int count = 0;
        /// 
        /// async token =>
        /// {
        ///     await asyncFunc(++count, token);
        /// }
        /// </code>
        /// 
        /// Obviously, with consideration for concurrent increment.
        /// </remarks>
        public static Func<CancellationToken, Task> LongCount(Func<long, CancellationToken, Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            var index = 0L;
            return token => asyncFunc(Interlocked.Increment(ref index), token);
        }

        #endregion

        #region Never

        private static readonly Func<CancellationToken, Task> s_never = async token =>
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            using (token.Register(() => taskCompletionSource.SetResult(true)))
            {
                await taskCompletionSource.Task.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }
        };

        /// <summary>
        /// An asynchronous function that never returns.
        /// </summary>
        /// <returns>An asynchronous function that never returns.</returns>
        /// <remarks>
        /// The resulting asynchronous function will throw if cancellation is requested.
        /// </remarks>
        public static Func<CancellationToken, Task> Never()
        {
            return s_never;
        }

        #endregion

        #region Sequence

        /// <summary>
        /// Converts an asynchronous function into one that is called sequentially.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>
        /// An asynchronous function that serializes a sequence of overlapping calls.
        /// </returns>
        /// <remarks>
        /// This is similar to <see cref="Exclusive(Func{CancellationToken, Task})"/>,
        /// except non-exclusive calls will await the completion of the active
        /// task. It's conceptually similar to an asynchronous lock with first-in,
        /// first-out semantics.
        /// </remarks>
        public static Func<CancellationToken, Task> Sequence(Func<CancellationToken, Task> asyncFunc)
        {
            var activeTask = default(Task);

            return async token =>
            {
                var currentTask = new TaskCompletionSource<bool>();

                var lastTask = Interlocked.Exchange(ref activeTask, currentTask.Task);
                if (lastTask != null)
                {
                    await lastTask.ConfigureAwait(false);
                }

                try
                {
                    await asyncFunc(token).ConfigureAwait(false);
                }
                finally
                {
                    // TODO: Consider propagating cancellation semantics.
                    currentTask.SetResult(true);
                }
            };
        }

        #endregion

        #region Single

        /// <summary>
        /// Converts an asynchronous function into one that throws if called
        /// more than one time.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>
        /// An asynchronous function that can be called exactly once.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int count = 0;
        /// 
        /// async token =>
        /// {
        ///     if (count++ == 0)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        ///     else
        ///     {
        ///         throw new InvalidOperationException();
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Single(Func<CancellationToken, Task> asyncFunc)
        {
            return First(
                asyncFunc,
                Throw(new InvalidOperationException("Asynchronous function invoked more than one time.")));
        }
        
        #endregion

        #region Skip

        /// <summary>
        /// Converts an asynchronous function into one that is skipped for a
        /// given number of invocations.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="count">The number of times to skip.</param>
        /// <returns>An asynchronous function with skipped invocations.</returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int i = 0;
        /// 
        /// async token =>
        /// {
        ///     if (++i > count)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Skip(Func<CancellationToken, Task> asyncFunc, int count)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return Skip(asyncFunc, count, Empty());
        }

        /// <summary>
        /// Converts an asynchronous function into one that is skipped for a
        /// given number of invocations.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="count">The number of times to skip.</param>
        /// <param name="rest">The asynchronous function to invoke when skipped.</param>
        /// <returns>An asynchronous function with skipped invocations.</returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int i = 0;
        /// 
        /// async token =>
        /// {
        ///     if (++i > count)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        ///     else
        ///     {
        ///         await rest(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Skip(Func<CancellationToken, Task> asyncFunc, int count, Func<CancellationToken, Task> rest)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(rest, nameof(rest));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return Count(
                Condition(
                    Uncurry<int>(asyncFunc),
                    index => index > count, 
                    Uncurry<int>(rest)));
        }

        #endregion

        #region Switch

        /// <summary>
        /// Converts an asynchronous function into one that cancels the current
        /// invocation, awaits the cancellation, and starts over.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>
        /// An asynchronous function that restarts upon overlapping invocations.
        /// </returns>
        /// <remarks>
        /// This essentially represents "restart" semantics. Note, if the
        /// asynchronous function does not observe or otherwise obey the
        /// cancellation token, then the transform will have the same semantics
        /// as <see cref="Sequence(Func{CancellationToken, Task})"/>.
        /// </remarks>
        public static DisposableAsyncFunction Switch(Func<CancellationToken, Task> asyncFunc)
        {
            var gate = new object();
            var serialDisposable = new SerialDisposable();
            var activeTask = default(Task);
            var activeToken = default(CancellationTokenSource);

            return Create(
                async token =>
                {
                    var currentTask = new TaskCompletionSource<bool>();
                    var currentToken = new CancellationTokenSource();

                    var lastTask = default(Task);
                    var lastToken = default(CancellationTokenSource);

                    lock (gate)
                    {
                        lastTask = Interlocked.Exchange(ref activeTask, currentTask.Task);
                        lastToken = Interlocked.Exchange(ref activeToken, currentToken);
                    }

                    if (lastTask != null)
                    {
                        lastToken.Cancel();
                        await lastTask.ConfigureAwait(false);
                    }

                    serialDisposable.Disposable = currentToken;

                    try
                    {
                        using (token.Register(currentToken.Cancel))
                        {
                            await asyncFunc(currentToken.Token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // TODO: Consider propagating errors or cancellation.
                        currentTask.SetResult(true);
                    }
                },
                serialDisposable);
        }

        #endregion

        #region Take

        /// <summary>
        /// Converts an asynchronous function into one that is ignored after a
        /// given number of invocations.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="count">The invocation limit.</param>
        /// <returns>
        /// An asynchronous function with a limited number of invocations.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int i = 0;
        /// 
        /// async token =>
        /// {
        ///     if (i++ &lt; count)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Take(Func<CancellationToken, Task> asyncFunc, int count)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return Take(asyncFunc, count, Empty());
        }

        /// <summary>
        /// Converts an asynchronous function into one that is ignored after a
        /// given number of invocations.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="count">The invocation limit.</param>
        /// <param name="rest">The asynchronous function to invoke after the limit.</param>
        /// <returns>
        /// An asynchronous function with a limited number of invocations.
        /// </returns>
        /// <remarks>
        /// The transform has the same semantics as:
        /// 
        /// <code>
        /// int i = 0;
        /// 
        /// async token =>
        /// {
        ///     if (i++ &lt; count)
        ///     {
        ///         await asyncFunc(token);
        ///     }
        ///     else
        ///     {
        ///         await rest(token);
        ///     }
        /// }
        /// </code>
        /// 
        /// </remarks>
        public static Func<CancellationToken, Task> Take(Func<CancellationToken, Task> asyncFunc, int count, Func<CancellationToken, Task> rest)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(rest, nameof(rest));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return Count(
                Condition(
                    Uncurry<int>(asyncFunc),
                    x => x <= count,
                    Uncurry<int>(rest)));
        }

        #endregion

        #region Throw

        /// <summary>
        /// An asynchronous function that throws the given exception.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <returns>An asynchronous function.</returns>
        public static Func<CancellationToken, Task> Throw(Exception exception)
        {
            return _ =>
            {
                throw exception;
            };
        }

        #endregion

        #region Toggle

        /// <summary>
        /// Converts an asynchronous function into one that invokes the
        /// asynchronous functions if none are running, otherwise cancels the
        /// active task. Any further invocations that occur while awaiting the
        /// cancellation of a request are ignored.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <returns>A toggled asynchronous function.</returns>
        /// <remarks>
        /// The semantics of this transformation are conceptually similar to
        /// <see cref="Exclusive(Func{CancellationToken, Task})"/>, where the
        /// first overlapping invocation cancels the current task.
        /// 
        /// If cancellation is not wired up properly, or otherwise observed,
        /// this transformation has the same semantics as 
        /// <see cref="Exclusive(Func{CancellationToken, Task})"/>.
        /// </remarks>
        public static DisposableAsyncFunction Toggle(Func<CancellationToken, Task> asyncFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));

            return Toggle(asyncFunc, Empty());
        }

        /// <summary>
        /// Converts an asynchronous function into one that invokes the
        /// asynchronous functions if none are running, otherwise cancels the
        /// active task. Any further invocations that occur while awaiting the
        /// cancellation of a request are ignored.
        /// </summary>
        /// <param name="asyncFunc">An asynchronous function.</param>
        /// <param name="cancelingFunc">
        /// The asynchronous function to invoke while awaiting cancellation.
        /// </param>
        /// <returns>A toggled asynchronous function.</returns>
        /// <remarks>
        /// The semantics of this transformation are conceptually similar to
        /// <see cref="Exclusive(Func{CancellationToken, Task})"/>, where the
        /// first overlapping invocation cancels the current task.
        /// 
        /// If cancellation is not wired up properly, or otherwise observed,
        /// this transformation has the same semantics as 
        /// <see cref="Exclusive(Func{CancellationToken, Task}, Func{CancellationToken, Task})"/>,
        /// where the first non-exclusive  is skipped, as in
        /// <see cref="Skip(Func{CancellationToken, Task}, int)"/>; i.e.,
        /// 
        /// <code>
        /// Toggle(asyncFunc, cancelingFunc) == Exclusive(asyncFunc, Skip(cancelingFunc, 1));
        /// </code>
        /// </remarks>
        public static DisposableAsyncFunction Toggle(Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> cancelingFunc)
        {
            Requires(asyncFunc, nameof(asyncFunc));
            Requires(cancelingFunc, nameof(cancelingFunc));

            var invalidGate = default(object);
            var tokenGate = new object();
            var cancellationTokenSource = default(CancellationTokenSource);
            var serialDisposable = new SerialDisposable();

            return Create(
                Exclusive(async token =>
                {
                    lock (tokenGate)
                    {
                        cancellationTokenSource = new CancellationTokenSource();
                        serialDisposable.Disposable = cancellationTokenSource;
                    }

                    using (token.Register(cancellationTokenSource.Cancel))
                    {
                        await asyncFunc(cancellationTokenSource.Token).ConfigureAwait(false);
                    }

                    lock (tokenGate)
                    {
                        cancellationTokenSource = null;
                        invalidGate = null;
                    }
                },
                async token =>
                {
                    var cancelled = false;
                    lock (tokenGate)
                    {
                        if (Interlocked.CompareExchange(ref invalidGate, new object(), null) == null && cancellationTokenSource != null)
                        {
                            cancellationTokenSource.Cancel();
                            cancelled = true;
                        }
                    }

                    if (!cancelled)
                    {
                        await cancelingFunc(token).ConfigureAwait(false);
                    }
                }),
                serialDisposable);
        }

        #endregion

        #region Helpers

        private static Func<T, TResult> Uncurry<T, TResult>(Func<TResult> func)
        {
            return arg => func();
        }

        private static Func<T, CancellationToken, Task> Uncurry<T>(Func<CancellationToken, Task> asyncFunc)
        {
            return (arg, token) => asyncFunc(token);
        }

        private static DisposableAsyncFunction Create(Func<CancellationToken, Task> asyncFunc, IDisposable disposable)
        {
            return new DisposableAsyncFunction(asyncFunc, disposable);
        }

        private static void Requires<T>(T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        #endregion
    }
}
