using HouseholdBudget.DTO.Transaction;

namespace HouseholdBudget.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionListResponseDTO>> GetAllAsync();
        Task<IEnumerable<TransactionListResponseDTO>> AdminGetAllAsync();
        Task<TransactionDetailResponseDTO?> GetByIdAsync(int id);
        Task<TransactionDetailResponseDTO?> AdminGetByIdAsync(int id);
        Task<TransactionDetailResponseDTO> AddAsync(TransactionCreateRequestDTO transactionDto);
        Task<TransactionDetailResponseDTO> UpdateAsync(TransactionUpdateRequestDTO transactionDto);
        Task DeleteAsync(int id);
    }
}
