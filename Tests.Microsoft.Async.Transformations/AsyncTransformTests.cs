using Microsoft.Async.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.Async.Transformations.AsyncTransform;

namespace Tests.Microsoft.Async.Transformations
{
    [TestClass]
    public partial class AsyncTransformTests
    {
        [TestMethod]
        public async Task AsyncTransform_AsAsync()
        {
            var count = 0;
            var enter = new AutoResetEvent(false);
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => { enter.WaitOne(); count++; });

            var task1 = Task.Run(() => asyncFunc(CancellationToken.None));
            var task2 = Task.Run(() => asyncFunc(CancellationToken.None));

            Assert.AreEqual(0, count);
            enter.Set();
            var task = await Task.WhenAny(task1, task2);
            await task;
            Assert.AreEqual(1, count);

            enter.Set();
            await Task.WhenAll(task1, task2);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_AsAsync_Cancelled()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                await asyncFunc(cts.Token);
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_AsCancellable()
        {
            var asyncFunc = AsyncTransformExtensions.AsCancellable(() => Task.FromResult(true));
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                await asyncFunc(cts.Token);
            }

            // No exceptions implies success
        }

        [TestMethod]
        public async Task AsyncTransform_Condition()
        {
            var count = 0;

            var asyncFunc1 = AsyncTransformExtensions.AsAsync(() => count += 1);
            var asyncFunc2 = AsyncTransformExtensions.AsAsync(() => count += 2);

            var test1 = asyncFunc1.Condition(() => count == 0, asyncFunc2);
            await test1(CancellationToken.None);
            await test1(CancellationToken.None);
            Assert.AreEqual(3, count);

            count = 0;
            var test2 = asyncFunc1.Condition(() => count == 0);
            await test2(CancellationToken.None);
            await test2(CancellationToken.None);
            Assert.AreEqual(1, count);

            count = 0;
            var test3 = Uncurry<int>(asyncFunc1).Condition<int>(i => i == 0, Uncurry<int>(asyncFunc2));
            await test3(0, CancellationToken.None);
            await test3(1, CancellationToken.None);
            Assert.AreEqual(3, count);

            count = 0;
            var test4 = Uncurry<int>(asyncFunc1).Condition<int>(i => i == 0);
            await test4(0, CancellationToken.None);
            await test4(1, CancellationToken.None);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Count()
        {
            var gate = new object();
            var sum = 0;
            var asyncFunc = AsyncTransformExtensions.Count(async (i, token) =>
            {
                await Task.Yield();
                lock (gate)
                {
                    sum += i;
                }
            });

            var n = 50000;
            var tasks = new List<Task>((int)n);
            for (var i = 0; i < n; ++i)
            {
                tasks.Add(asyncFunc(CancellationToken.None));
            }

            await Task.WhenAll(tasks);

            Assert.AreEqual(n * ((n + 1) / 2) + ((n / 2) * ((n + 1) % 2)), sum);
        }

        [TestMethod]
        public async Task AsyncTransform_Curry()
        {
            var count = 0;
            var expected = 42;
            Func<int, CancellationToken, Task> asyncFunc = (actual, token) =>
            {
                Assert.AreEqual(expected, actual);
                ++count;
                return Task.FromResult(true);
            };

            var curried = asyncFunc.Curry(expected);
            await curried(CancellationToken.None);
            await curried(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Empty()
        {
            await Empty()(CancellationToken.None);

            var asyncFunc = Empty();
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                await asyncFunc(cts.Token);
            }

            // No exceptions implies success
        }

        [TestMethod]
        public async Task AsyncTransform_Exclusive()
        {
            var count = 0;

            var are = new AutoResetEvent(false);
            var tcs = new TaskCompletionSource<bool>();
            var test = AsyncTransformExtensions.AsCancellable(() => { count++; are.Set(); return tcs.Task; });

            var exclusive = test.Exclusive();
            var task = exclusive(CancellationToken.None);

            are.WaitOne();
            Assert.AreEqual(1, count);

            // empty action
            await exclusive(CancellationToken.None);

            Assert.IsFalse(task.IsCompleted);
            tcs.SetResult(true);
            await task;

            await exclusive(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Exclusive_NonExclusive()
        {
            var count = 0;

            var test = AsyncTransformExtensions.AsAsync(() => count++);

            using (var cts = new CancellationTokenSource())
            {
                var exclusive = Never().Exclusive(test);
                var task = exclusive(cts.Token);

                await exclusive(CancellationToken.None);
                await exclusive(CancellationToken.None);
                await exclusive(CancellationToken.None);

                Assert.AreEqual(3, count);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Finally()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);

            var ex = new Exception();
            var test1 = Throw(ex).Finally(asyncFunc);
            var test2 = Empty().Finally(asyncFunc);
            await AssertEx.Throws<Exception>(() => test1(CancellationToken.None), e => Assert.AreSame(ex, e));
            await test2(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_First()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);
            var first = asyncFunc.First();
            await first(CancellationToken.None);
            await first(CancellationToken.None);
            await first(CancellationToken.None);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AsyncTransform_First_Rest()
        {
            var count = 0;
            var task = AsyncTransformExtensions.AsAsync(() => count++);
            var first = Empty().First(task);
            await first(CancellationToken.None);
            await first(CancellationToken.None);
            await first(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_LongCount()
        {
            var gate = new object();
            var sum = 0L;

            var asyncFunc = AsyncTransformExtensions.LongCount(async (i, token) =>
            {
                await Task.Yield();
                lock (gate)
                {
                    sum += i;
                }
            });

            var n = 100000L;
            var tasks = new List<Task>((int)n);
            for (var i = 0L; i < n; ++i)
            {
                tasks.Add(asyncFunc(CancellationToken.None));
            }

            await Task.WhenAll(tasks);

            Assert.AreEqual(n * ((n + 1) / 2) + ((n / 2) * ((n + 1) % 2)), sum);
        }

        [TestMethod]
        public async Task AsyncTransform_Never()
        {
            using (var cts = new CancellationTokenSource())
            {
                var task = Never()(cts.Token);
                cts.Cancel();
                await AssertEx.Throws<OperationCanceledException>(
                    () => task,
                    ex => Assert.AreEqual(cts.Token, ex.CancellationToken));
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Sequence()
        {
            var enter = new AutoResetEvent(false);
            var exit = new AutoResetEvent(false);
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => 
            {
                enter.Set();
                exit.WaitOne();
                return;
            });

            var test = asyncFunc.Sequence();

            var task1 = Task.Run(() => test(CancellationToken.None));
            enter.WaitOne();

            var task2 = Task.Run(() => test(CancellationToken.None));

            Assert.IsFalse(enter.WaitOne(100));
            Assert.IsFalse(task1.IsCompleted);
            Assert.IsFalse(task2.IsCompleted);

            exit.Set();
            enter.WaitOne();
            exit.Set();

            await task1;
            await task2;
        }

        [TestMethod]
        public async Task AsyncTransform_Sequence_Cancellation()
        {
            var test = Never().Sequence();
            using (var cts1 = new CancellationTokenSource())
            using (var cts2 = new CancellationTokenSource())
            {
                var task1 = test(cts1.Token);
                var task2 = test(cts2.Token);

                cts1.Cancel();
                await AssertEx.Throws<OperationCanceledException>(() => task1, ex => Assert.AreEqual(cts1.Token, ex.CancellationToken));

                cts2.Cancel();
                await AssertEx.Throws<OperationCanceledException>(() => task2, ex => Assert.AreEqual(cts2.Token, ex.CancellationToken));
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Single()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);

            var test = asyncFunc.Single();
            await test(CancellationToken.None);
            await AssertEx.Throws<InvalidOperationException>(() => test(CancellationToken.None));
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Skip()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);

            var test = asyncFunc.Skip(1);
            await test(CancellationToken.None);
            Assert.AreEqual(0, count);
            await test(CancellationToken.None);
            Assert.AreEqual(1, count);
            await test(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Skip_Rest()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);

            var test = Empty().Skip(1, asyncFunc);
            await test(CancellationToken.None);
            Assert.AreEqual(1, count);
            await test(CancellationToken.None);
            Assert.AreEqual(1, count);
            await test(CancellationToken.None);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Switch()
        {
            var enter = new AutoResetEvent(false);
            var exit = new AutoResetEvent(false);

            var count = 0;
            var asyncFunc = Identity(async token =>
            {
                await Task.Yield();
                enter.Set();
                exit.WaitOne();
                if (!token.IsCancellationRequested)
                {
                    count++;
                }
                await Task.FromResult(false);
            });

            using (var disposableAction = asyncFunc.Switch())
            {
                var @switch = Identity(disposableAction.InvokeAsync);
                var task1 = @switch(CancellationToken.None);
                enter.WaitOne();
                exit.Set();
                await task1;
                Assert.AreEqual(1, count);

                var task2 = @switch(CancellationToken.None);
                enter.WaitOne();
                var task3 = @switch(CancellationToken.None);

                exit.Set();
                await task2;
                Assert.AreEqual(1, count);

                enter.WaitOne();
                exit.Set();
                await task3;
                Assert.AreEqual(2, count);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Switch_Cancellation()
        {
            using (var cts = new CancellationTokenSource())
            {
                var are = new AutoResetEvent(false);
                var count = 0;

                var neverish = Identity(async token =>
                {
                    are.Set();
                    count++;
                    while (true)
                    {
                        await Task.Delay(100);
                        cts.Token.ThrowIfCancellationRequested();
                    }
                });

                using (var test = neverish.Switch())
                {
                    var task1 = test.InvokeAsync(CancellationToken.None);
                    var task2 = test.InvokeAsync(CancellationToken.None);

                    Assert.IsTrue(are.WaitOne());
                    Assert.AreEqual(1, count);
                    Assert.IsFalse(are.WaitOne(100));

                    cts.Cancel();

                    await AssertEx.Throws<OperationCanceledException>(() => task1, ex => Assert.AreEqual(cts.Token, ex.CancellationToken));
                    await AssertEx.Throws<OperationCanceledException>(() => task2, ex => Assert.AreEqual(cts.Token, ex.CancellationToken));
                    Assert.AreEqual(2, count);
                }
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Switch_Disposed()
        {
            var test = Never().Switch();
            test.Dispose();
            await AssertEx.Throws<ObjectDisposedException>(() => test.InvokeAsync(CancellationToken.None));
        }

        [TestMethod]
        public async Task AsyncTransform_Switch_Fast()
        {
            var enter = new AutoResetEvent(false);
            var exit = new AutoResetEvent(false);

            var count = 0;
            var asyncFunc = Identity(async token =>
            {
                await Task.Yield();
                enter.Set();
                exit.WaitOne();
                if (!token.IsCancellationRequested)
                {
                    count++;
                }
                await Task.FromResult(false);
            });

            using (var disposableAction = asyncFunc.Switch())
            {
                var @switch = Identity(disposableAction.InvokeAsync);
                var task1 = @switch(CancellationToken.None);
                enter.WaitOne();
                exit.Set();
                await task1;
                Assert.AreEqual(1, count);

                var task2 = @switch(CancellationToken.None);
                enter.WaitOne();

                var tasks = new List<Task>
                {
                    @switch(CancellationToken.None),
                    @switch(CancellationToken.None),
                };

                exit.Set();
                await task2;
                Assert.AreEqual(1, count);

                while (tasks.Count > 0)
                {
                    enter.WaitOne();
                    exit.Set();
                    var anyTask = await Task.WhenAny(tasks);
                    tasks.Remove(anyTask);
                    await anyTask;
                }

                Assert.IsTrue(count >= 2);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Switch_Never()
        {
            var toggle = Never().Switch();
            var task1 = toggle.InvokeAsync(CancellationToken.None);
            var task2 = toggle.InvokeAsync(CancellationToken.None);
            var task = await Task.WhenAny(task1, task2);
            var otherTask = task == task1 ? task2 : task1;
            toggle.Dispose();
            await AssertEx.Throws<ObjectDisposedException>(() => otherTask);
        }

        [TestMethod]
        public async Task AsyncTransform_SwitchMany_Multiple()
        {
            var count = 2;

            var enters = Create(() => new AutoResetEvent(false), count);
            var exits = Create(() => new AutoResetEvent(false), count);
            var cancelFlags = Enumerable.Repeat(false, count).ToList();
            var tasks = enters.Select((enter, i) => Identity(async token =>
            {
                enter.Set();

                var tcs = new TaskCompletionSource<bool>();
                using (token.Register(() => tcs.SetResult(false)))
                {
                    var task = Task.Run(() => { exits[i].WaitOne(); });
                    await Task.WhenAny(task, tcs.Task);
                }

                cancelFlags[i] = token.IsCancellationRequested;
            })).ToArray();

            var switchedTasks = SwitchMany(tasks);

            exits[0].Set();
            await switchedTasks[0].InvokeAsync(CancellationToken.None);
            enters[0].WaitOne();
            Assert.IsFalse(cancelFlags[0]);

            var t0 = switchedTasks[0].InvokeAsync(CancellationToken.None);
            enters[0].WaitOne();
            var t1 = switchedTasks[1].InvokeAsync(CancellationToken.None);
            enters[1].WaitOne();
            exits[0].Set();
            await t0;
            Assert.IsTrue(cancelFlags[0]);
            exits[1].Set();
            await t1;
            Assert.IsFalse(cancelFlags[1]);

            cancelFlags[0] = false;

            var t0a = switchedTasks[0].InvokeAsync(CancellationToken.None);
            enters[0].WaitOne();
            var t0b = switchedTasks[0].InvokeAsync(CancellationToken.None);
            enters[0].WaitOne();
            exits[0].Set();
            await Task.WhenAny(t0a, t0b);
            exits[0].Set();
            await t0a;
            await t0b;
            Assert.IsFalse(cancelFlags[0]);

            switchedTasks.ToList().ForEach(d => d.Dispose());
        }

        [TestMethod]
        public async Task AsyncTransform_SwitchMany_Cancellation()
        {
            var tasks = SwitchMany(Enumerable.Repeat(Never(), 1)).ToArray();
            using (var cts = new CancellationTokenSource())
            {
                var task = tasks[0].InvokeAsync(cts.Token);
                cts.Cancel();
                await AssertEx.Throws<OperationCanceledException>(() => task, ex => Assert.AreEqual(cts.Token, ex.CancellationToken));
            }
        }

        [TestMethod]
        public async Task AsyncTransform_SwitchMany_Dispose()
        {
            var asyncFuncs = SwitchMany(Never(), Never());
            asyncFuncs[0].Dispose();
            await AssertEx.Throws<ObjectDisposedException>(() => asyncFuncs[0].InvokeAsync(CancellationToken.None));
            var task = asyncFuncs[1].InvokeAsync(CancellationToken.None);
            asyncFuncs[1].Dispose();
            await AssertEx.Throws<ObjectDisposedException>(() => task);
        }

        [TestMethod]
        public async Task AsyncTransform_SwitchMany_Exception()
        {
            var ex = new OperationCanceledException();
            var asyncFuncs = SwitchMany(Throw(ex));
            await AssertEx.Throws<OperationCanceledException>(() => asyncFuncs[0].InvokeAsync(CancellationToken.None), e => Assert.AreSame(ex, e));
        }

        [TestMethod]
        public async Task AsyncTransform_SwitchMany_NoException()
        {
            var asyncFuncs = SwitchMany(Never(), Never()).ToList();
            var t1 = asyncFuncs[0].InvokeAsync(CancellationToken.None);
            var t2 = asyncFuncs[1].InvokeAsync(CancellationToken.None);
            var task = await Task.WhenAny(t1, t2);
            var otherTask = task == t1 ? t2 : t1;
            asyncFuncs.ForEach(d => d.Dispose());
            await AssertEx.Throws<ObjectDisposedException>(() => otherTask);
        }

        [TestMethod]
        public async Task AsyncTransform_Take()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);
            var take = asyncFunc.Take(1);
            await take(CancellationToken.None);
            await take(CancellationToken.None);
            await take(CancellationToken.None);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Take_Rest()
        {
            var count = 0;
            var asyncFunc = AsyncTransformExtensions.AsAsync(() => count++);
            var take = Empty().Take(1, asyncFunc);
            await take(CancellationToken.None);
            await take(CancellationToken.None);
            await take(CancellationToken.None);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Throw()
        {
            var exception = new Exception();
            var asyncAction = Throw(exception);
            await AssertEx.Throws<Exception>(() => asyncAction(CancellationToken.None), ex => Assert.AreSame(exception, ex));
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle()
        {
            var enter = new AutoResetEvent(false);
            var exit = new AutoResetEvent(false);

            var count = 0;
            var asyncFunc = Identity(async token =>
            {
                await Task.Yield();
                enter.Set();
                exit.WaitOne();
                if (!token.IsCancellationRequested)
                {
                    count++;
                }
                await Task.FromResult(false);
            });

            using (var disposableAction = asyncFunc.Toggle())
            {
                var toggle = Identity(disposableAction.InvokeAsync);

                var task1 = toggle(CancellationToken.None);
                enter.WaitOne();
                exit.Set();
                await task1;
                Assert.AreEqual(1, count);

                var task2 = toggle(CancellationToken.None);
                enter.WaitOne();
                var task3 = toggle(CancellationToken.None);

                // empty task
                var task4 = toggle(CancellationToken.None);
                await task4;

                exit.Set();
                await task2;
                Assert.AreEqual(1, count);

                await task3;
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle_Cancellation()
        {
            var enter = new AutoResetEvent(false);
            var exit = new AutoResetEvent(false);

            var count = 0;
            var asyncFunc = Identity(async token =>
            {
                await Task.Yield();
                enter.Set();
                exit.WaitOne();
                if (!token.IsCancellationRequested)
                {
                    count++;
                }
                await Task.FromResult(false);
            });

            using (var disposableAction = asyncFunc.Toggle())
            {
                var toggle = Identity(disposableAction.InvokeAsync);

                var task1 = toggle(CancellationToken.None);
                enter.WaitOne();
                exit.Set();
                await task1;
                Assert.AreEqual(1, count);

                var task2 = toggle(CancellationToken.None);
                enter.WaitOne();
                var task3 = toggle(CancellationToken.None);

                // empty task
                var task4 = toggle(CancellationToken.None);
                await task4;

                exit.Set();
                await task2;
                Assert.AreEqual(1, count);

                await task3;
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle_CancelingFunc()
        {
            var count = 0;
            var enter = new ManualResetEvent(false);
            var exit = new ManualResetEvent(false);

            var blocking1 = AsAsync(() => { enter.Set(); exit.WaitOne(); });
            var cancelingFunc = AsAsync(() => count++);

            using (var toggle = blocking1.Toggle(cancelingFunc))
            {
                var task1 = Task.Run(() => toggle.InvokeAsync(CancellationToken.None));
                enter.WaitOne();

                var task2 = toggle.InvokeAsync(CancellationToken.None);

                await toggle.InvokeAsync(CancellationToken.None);
                await toggle.InvokeAsync(CancellationToken.None);

                exit.Set();
                await task1;
                await task2;
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle_Never()
        {
            var toggle = Never().Toggle();
            var task1 = toggle.InvokeAsync(CancellationToken.None);
            var task2 = toggle.InvokeAsync(CancellationToken.None);
            await Task.WhenAll(task1, task2);

            // No exception implies success
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle_Disposed()
        {
            var toggle = Never().Toggle();
            toggle.Dispose();
            await AssertEx.Throws<ObjectDisposedException>(() => toggle.InvokeAsync(CancellationToken.None));
        }

        [TestMethod]
        public async Task AsyncTransform_Toggle_Exception()
        {
            var ex = new OperationCanceledException();
            var asyncFunc = Throw(ex).Toggle();
            await AssertEx.Throws<OperationCanceledException>(() => asyncFunc.InvokeAsync(CancellationToken.None), e => Assert.AreSame(ex, e));
        }

        private static Action<ArgumentException> AssertName(string paramName)
        {
            return ex => Assert.AreEqual(paramName, ex.ParamName);
        }

        private static Func<T, CancellationToken, Task> Uncurry<T>(Func<CancellationToken, Task> asyncFunc)
        {
            return (_, token) => asyncFunc(token);
        }

        private static IList<T> Create<T>(Func<T> factory, int count)
        {
            return Enumerable.Repeat(factory, count).Select(f => f()).ToList();
        }
    }
}
