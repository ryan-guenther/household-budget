using HouseholdBudget.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

public class DbTransactionManager : IDbTransactionManager
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _isCommittedOrRolledBack;

    public DbTransactionManager(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IDbTransactionManager> BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _transaction = await _context.Database.BeginTransactionAsync();
        _isCommittedOrRolledBack = false;
        return this;
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        if (_isCommittedOrRolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");

        await _transaction.CommitAsync();
        _isCommittedOrRolledBack = true;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to roll back.");

        if (_isCommittedOrRolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");

        await _transaction.RollbackAsync();
        _isCommittedOrRolledBack = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            if (!_isCommittedOrRolledBack)
            {
                // Rollback any uncommitted transaction before disposing
                await _transaction.RollbackAsync();
            }

            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
