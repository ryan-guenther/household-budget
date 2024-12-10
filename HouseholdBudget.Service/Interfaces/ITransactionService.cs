using HouseholdBudget.DTO;

namespace HouseholdBudget.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetAllAsync();
        Task<TransactionDTO?> GetByIdAsync(int id);
        Task AddAsync(TransactionDTO transactionDto);
        Task UpdateAsync(TransactionDTO transactionDto);
        Task DeleteAsync(int id);
    }
}
