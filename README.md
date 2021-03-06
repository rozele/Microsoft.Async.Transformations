# Microsoft.Async.Transformations

A set of higher-order functions, which transform asynchronous functions of the form `Func<CancellationToken, Task>` into asynchronous functions with additional semantics.

For example, one of the transformations, `Switch`, creates an asynchronous function where each time it is called, it cancels the previous call.

# Usage

Install the Nuget packages using your favorite NuGet tools:
 * [Microsoft.Async.Transformations](https://www.nuget.org/packages/Microsoft.Async.Transformations)
 * [Microsoft.Async.Transformations.Windows](https://www.nuget.org/packages/Microsoft.Async.Transformations.Windows)

As a matter of convenience, we've created both static methods and extension methods for most of the transformations where it made sense to do so. To use the extension methods, be sure to include the `Microsoft.Async.Transformations` namespace.The extension methods are available in `Microsoft.Async.Transformations.AsyncTransformExtensions` and the non-extension static methods are in `Microsoft.Async.Transformations.AsyncTransform`. In the example app and the code snippets below, we leverage the "Using Static" feature from C# 6.0 (see more [here](https://msdn.microsoft.com/en-us/magazine/dn879355.aspx)), i.e.:
```
using static Microsoft.Async.Transformations.AsyncTransform;
```

# Example

The example included in Playground.App is a simple stopwatch application intended to demonstrate some of the interesting transformations.

The stopwatch has 3 basic functions:
  1. A toggle switch to start and pause the stopwatch.
  2. A reset button to reset the stopwatch.
  3. A restart button to restart the stopwatch.

All of the functionality is implemented from a dead simple asynchronous function that waits one second, then calls a "tick" function, until the loop is cancelled by the input cancellation token. Here's the code:

```
public async Task RunAsync(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        try
        {
            await Task.Delay(s_interval, token);
            await _tickAsync(token);
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken != token)
            {
                throw;
            }
        }
    }
}
```

To implement the toggle switch, we simply used a transform on this `RunAsync` function a la:
```
var startStopCoreAsync = Identity(Context.RunAsync).Toggle();
```
Notice here we use the `Identity` function as syntactic sugar, albeit at the price of a wasted method call. The optimal way to write this code is simply:
```
var startStopCoreAsync = new Func<CancellationToken, Task>(Context.RunAsync).Toggle();
```
Or even:
```
Func<CancellationToken, Task> asyncFunc = Context.RunAsync;
var startStopCoreAsync = asyncFunc.Toggle();
```
But nonetheless, this is only for demo purposes and we'll use the `Identity` function throughout.

Now, the functionality of `_tickAsync` is to simply increment the current value of the `Elapsed` property on the context. The `Elapsed` property is hooked up via the `System.ComponentModel.INotifyPropertyChanged` and the binding mechanism in XAML, so we need to make sure we upodate the `Elapsed` property on a dispatcher thread.  This is where the transformation from `Microsoft.Async.Transformations` comes in handy. We use the `AsAsync` transformation to treat the `Tick` method as an asynchronous one, then use the `OnDispatcher` transformation to ensure function is called from a dispatcher thread (assuming the caller to OnDispatcher is itself a dispatcher thread):
```
public StopwatchContext()
{
    _tickAsync = AsAsync(Tick).OnDispatcher();
    ...
}

...

private void Tick()
{
    Elapsed += s_interval;
}
```

We implement the Restart functionality using the same `RunAsync` method that the toggle button works with. Using the `Switch` transformation, the behavior of restart is to simply cancel the current call to `RunAsync` and start it over again (with the added behavior of "resetting" the `Elapsed` property prior to calling `RunAsync`).

Obviously, there are more efficient ways to implement restart (like allowing the `RunAsync` method to continue running while simply resetting the `Elapsed` property), but then we couldn't demo the `Switch` transformation.

The point is, there are many cases in XAML applications where you want to implement asynchronous functionality for toggle switches, or create buttons that simply restart any active asynchronous process, or create buttons that run only the first time the user presses them, or cancels other activities in the application before starting the expected activity. That is what the `Microsoft.Async.Transformation` library is designed for. The ultimate goal is to allow developers to write functional asynchronous behavior that is easier to reason about and more concise to develop.

## Dependencies
* [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)

## Contributions
Contributions are awesome! If you want to help out, fix a bug, or get a feature fixed - check out the [contributing guide](CONTRIBUTING.md). 
