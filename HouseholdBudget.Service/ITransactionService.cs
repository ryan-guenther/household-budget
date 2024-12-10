using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Service.Interfaces;

using Mapster;

namespace HouseholdBudget.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<TransactionDTO>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return transactions.Adapt<IEnumerable<TransactionDTO>>();  // Automatically map from Transaction to TransactionDTO
        }

        public async Task<TransactionDTO?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            return transaction?.Adapt<TransactionDTO>();  // Automatically map from Transaction to TransactionDTO
        }

        public async Task AddAsync(TransactionDTO transactionDto)
        {
            var transaction = transactionDto.Adapt<Transaction>();  // Automatically map from TransactionDTO to Transaction
            await _transactionRepository.AddAsync(transaction);
        }

        public async Task UpdateAsync(TransactionDTO transactionDto)
        {
            var transaction = transactionDto.Adapt<Transaction>();  // Automatically map from TransactionDTO to Transaction
            await _transactionRepository.UpdateAsync(transaction);
        }

        public async Task DeleteAsync(int id)
        {
            await _transactionRepository.DeleteAsync(id);
        }
    }
}
