This is a placeholder for describing how we will implement "Async All The Way", including answers to questions like:

1. Do we put `IAsyncDisposable` interfaces in the same file as `IDisposable`, or do we use `partial class`es to keep the two separate?
2. Do we support `netstandard2.1` so that we can utilize `await using` syntax internally, or just something we allow consumers to use, e.g. inside a Migration and when composing Migrations (implementing their own TaskExecutor)?
3. Documenting which database drivers support async interfaces
4. Returning `Task.CompletedTask` when the underlying database drivers don't suppose async interfaces.
