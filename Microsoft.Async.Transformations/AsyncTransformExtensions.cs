using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Async.Transformations
{
    /// <summary>
    /// A set of extensions for asynchronous functions.
    /// </summary>
    public static class AsyncTransformExtensions
    {
        #region AsAsync

        /// <summary>
        /// Wraps an action as a asynchronous function.
        /// </summary>
        /// <param name="action">An action.</param>
        /// <returns>An asynchronous function wrapper around the action.</returns>
        /// <remarks>The cancellation token is ignored.</remarks>
        public static Func<CancellationToken, Task> AsAsync(this Action action)
        {
            return AsyncTransform.AsAsync(action);
        }

        #endregion

        #region AsCancellable

        /// <summary>
        /// Wraps an asynchronous function without cancellation as one with a
        /// cancellation token.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <returns>A cancellable wrapper around the asynchronous function.</returns>
        public static Func<CancellationToken, Task> AsCancellable(this Func<Task> asyncFunc)
        {
            return AsyncTransform.AsCancellable(asyncFunc);
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
        public static Func<CancellationToken, Task> Condition(this Func<CancellationToken, Task> @if, Func<bool> predicate)
        {
            return AsyncTransform.Condition(@if, predicate);
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
        public static Func<CancellationToken, Task> Condition(this Func<CancellationToken, Task> @if, Func<bool> predicate, Func<CancellationToken, Task> @else)
        {
            return AsyncTransform.Condition(@if, predicate, @else);
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
        public static Func<T, CancellationToken, Task> Condition<T>(this Func<T, CancellationToken, Task> @if, Func<T, bool> predicate)
        {
            return AsyncTransform.Condition(@if, predicate);
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
        public static Func<T, CancellationToken, Task> Condition<T>(this Func<T, CancellationToken, Task> @if, Func<T, bool> predicate, Func<T, CancellationToken, Task> @else)
        {
            return AsyncTransform.Condition(@if, predicate, @else);
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
        public static Func<CancellationToken, Task> Count(this Func<int, CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Count(asyncFunc);
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
        public static Func<CancellationToken, Task> Curry<T>(this Func<T, CancellationToken, Task> asyncFunc, T arg)
        {
            return AsyncTransform.Curry(asyncFunc, arg);
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
        public static Func<CancellationToken, Task> Exclusive(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Exclusive(asyncFunc);
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
        /// </remarks>
        public static Func<CancellationToken, Task> Exclusive(this Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> rest)
        {
            return AsyncTransform.Exclusive(asyncFunc, rest);
        }

        #endregion

        #region Finally

        /// <summary>
        /// Chains an asynchronous function with a function to invoke regardless of the outcome.
        /// </summary>
        /// <param name="asyncFunc">The asynchronous function.</param>
        /// <param name="finallyAction">The function to invoke to take regardless of outcome.</param>
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
        public static Func<CancellationToken, Task> Finally(this Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> finallyAction)
        {
            return AsyncTransform.Finally(asyncFunc, finallyAction);
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
        public static Func<CancellationToken, Task> First(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.First(asyncFunc);
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
        public static Func<CancellationToken, Task> First(this Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> rest)
        {
            return AsyncTransform.First(asyncFunc, rest);
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
        public static Func<CancellationToken, Task> LongCount(this Func<long, CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.LongCount(asyncFunc);
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
        public static Func<CancellationToken, Task> Sequence(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Sequence(asyncFunc);
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
        public static Func<CancellationToken, Task> Single(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Single(asyncFunc);
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
        public static Func<CancellationToken, Task> Skip(this Func<CancellationToken, Task> asyncFunc, int count)
        {
            return AsyncTransform.Skip(asyncFunc, count);
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
        public static Func<CancellationToken, Task> Skip(this Func<CancellationToken, Task> asyncFunc, int count, Func<CancellationToken, Task> rest)
        {
            return AsyncTransform.Skip(asyncFunc, count, rest);
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
        public static DisposableAsyncFunction Switch(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Switch(asyncFunc);
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
        public static Func<CancellationToken, Task> Take(this Func<CancellationToken, Task> asyncFunc, int count)
        {
            return AsyncTransform.Take(asyncFunc, count);
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
        public static Func<CancellationToken, Task> Take(this Func<CancellationToken, Task> asyncFunc, int count, Func<CancellationToken, Task> rest)
        {
            return AsyncTransform.Take(asyncFunc, count, rest);
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
        public static DisposableAsyncFunction Toggle(this Func<CancellationToken, Task> asyncFunc)
        {
            return AsyncTransform.Toggle(asyncFunc);
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
        /// where the first non-exclusive invocation is skipped, as in
        /// <see cref="Skip(Func{CancellationToken, Task}, int)"/>; i.e.,
        /// 
        /// <code>
        /// Toggle(asyncFunc, cancelingFunc) == Exclusive(asyncFunc, Skip(cancelingFunc, 1));
        /// </code>
        /// 
        /// </remarks>
        public static DisposableAsyncFunction Toggle(this Func<CancellationToken, Task> asyncFunc, Func<CancellationToken, Task> cancelingFunc)
        {
            return AsyncTransform.Toggle(asyncFunc, cancelingFunc);
        }

        #endregion
    }
}
