public interface IDbTransactionManager : IAsyncDisposable
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>The current transaction manager instance for chaining.</returns>
    Task<IDbTransactionManager> BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <returns>A task representing the asynchronous rollback operation.</returns>
    Task RollbackAsync();
}
