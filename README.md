# Microsoft.Async.Transformations

A set of higher-order functions, which transform asynchronous functions of the form `Func<CancellationToken, Task>` into asynchronous functions with additional semantics.

For example, one of the transformations, `Switch`, creates an asynchronous function where each time it is called, it cancels the previous call.

## Dependencies
* [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)
