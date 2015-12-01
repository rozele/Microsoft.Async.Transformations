using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Async.Transformations.AsyncTransform;

namespace Tests.Microsoft.Async.Transformations
{
    public partial class AsyncTransformTests
    {
        [TestMethod]
        public void AsyncTransform_ArgumentChecks()
        {
            AssertEx.Throws<ArgumentNullException>(() => AsAsync(null), AssertName("action"));

            AssertEx.Throws<ArgumentNullException>(() => AsCancellable(null), AssertName("asyncFunc"));

            AssertEx.Throws<ArgumentNullException>(() => Condition(null, () => true), AssertName("if"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty(), null), AssertName("predicate"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(null, () => true, Empty()), AssertName("if"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty(), null, Empty()), AssertName("predicate"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty(), () => true, null), AssertName("else"));
            AssertEx.Throws<ArgumentNullException>(() => Condition<int>(null, _ => true), AssertName("if"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty<int>(), null), AssertName("predicate"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(null, _ => true, Empty<int>()), AssertName("if"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty<int>(), null, Empty<int>()), AssertName("predicate"));
            AssertEx.Throws<ArgumentNullException>(() => Condition(Empty<int>(), _ => true, null), AssertName("else"));

            AssertEx.Throws<ArgumentNullException>(() => Count(null), AssertName("asyncFunc"));

            AssertEx.Throws<ArgumentNullException>(() => Curry(null, default(int)), AssertName("asyncFunc"));

            AssertEx.Throws<ArgumentNullException>(() => Exclusive(null), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentNullException>(() => Exclusive(Empty(), null), AssertName("rest"));

            AssertEx.Throws<ArgumentNullException>(() => Finally(null, Empty()), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentNullException>(() => Finally(Empty(), null), AssertName("finallyAction"));

            AssertEx.Throws<ArgumentNullException>(() => First(null), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentNullException>(() => First(Empty(), null), AssertName("rest"));

            AssertEx.Throws<ArgumentNullException>(() => LongCount(null), AssertName("asyncFunc"));

            AssertEx.Throws<ArgumentNullException>(() => Sequence(null), AssertName("asyncFunc"));
                                                         
            AssertEx.Throws<ArgumentNullException>(() => Single(null), AssertName("asyncFunc"));

            AssertEx.Throws<ArgumentNullException>(() => Skip(null, 0), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => Skip(Empty(), -1), AssertName("count"));
            AssertEx.Throws<ArgumentNullException>(() => Skip(null, 0, Empty()), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => Skip(Empty(), -1, Empty()), AssertName("count"));
            AssertEx.Throws<ArgumentNullException>(() => Skip(Empty(), -1, null), AssertName("rest"));

            AssertEx.Throws<ArgumentNullException>(() => Switch(default(Func<CancellationToken, Task>)), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentNullException>(() => Switch(default(Func<CancellationToken, Task>[])), AssertName("asyncFuncList"));
            AssertEx.Throws<ArgumentNullException>(() => Switch(default(IEnumerable<Func<CancellationToken, Task>>)), AssertName("asyncFuncCollection"));

            AssertEx.Throws<ArgumentNullException>(() => Take(null, 0), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => Take(Empty(), -1), AssertName("count"));
            AssertEx.Throws<ArgumentNullException>(() => Take(null, 0, Empty()), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => Take(Empty(), -1, Empty()), AssertName("count"));
            AssertEx.Throws<ArgumentNullException>(() => Take(Empty(), -1, null), AssertName("rest"));

            AssertEx.Throws<ArgumentNullException>(() => Throw(null), AssertName("exception"));

            AssertEx.Throws<ArgumentNullException>(() => Toggle(null), AssertName("asyncFunc"));
            AssertEx.Throws<ArgumentNullException>(() => Toggle(Empty(), null), AssertName("cancelingFunc"));
        }
    }
}
