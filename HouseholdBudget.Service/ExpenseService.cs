using HouseholdBudget.Domain.Entities;
using HouseholdBudget.Repository;
using HouseholdBudget.Repository.Interfaces;
using HouseholdBudget.Service.Interfaces;

namespace HouseholdBudget.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _expenseRepository.GetAllAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _expenseRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Expense expense)
        {
            await _expenseRepository.AddAsync(expense);
        }

        public async Task UpdateAsync(Expense expense)
        {
            await _expenseRepository.UpdateAsync(expense);
        }

        public async Task DeleteAsync(int id)
        {
            await _expenseRepository.DeleteAsync(id);
        }
    }
}
