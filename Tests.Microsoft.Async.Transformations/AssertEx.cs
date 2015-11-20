using System;
using System.Threading.Tasks;

namespace Tests.Microsoft.Async.Transformations
{
    static class AssertEx
    {
        public static void Throws<T>(Action action)
            where T : Exception
        {
            Throws<T>(action, _ => { });
        }

        public static void Throws<T>(Action action, Action<T> assert)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T ex)
            {
                assert(ex);
            }
        }

        public static async Task Throws<T>(Func<Task> action)
            where T : Exception
        {
            await Throws<T>(action, _ => { });
        }

        public static async Task Throws<T>(Func<Task> action, Action<T> assert)
            where T : Exception
        {
            try
            {
                await action();
            }
            catch (T ex)
            {
                assert(ex);
            }
        }
    }
}
