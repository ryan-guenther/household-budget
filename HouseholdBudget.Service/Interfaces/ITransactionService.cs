using HouseholdBudget.DTO.Transaction;

namespace HouseholdBudget.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionListResponseDTO>> GetAllAsync();
        Task<TransactionDetailResponseDTO?> GetByIdAsync(int id);
        Task<TransactionDetailResponseDTO> AddAsync(TransactionCreateRequestDTO transactionDto);
        Task<TransactionDetailResponseDTO> UpdateAsync(TransactionUpdateRequestDTO transactionDto);
        Task DeleteAsync(int id);
    }
}
