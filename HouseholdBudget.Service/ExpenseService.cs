using HouseholdBudget.Domain.Entities;
using HouseholdBudget.DTO;
using HouseholdBudget.Repository;
using HouseholdBudget.Repository.Interfaces;
using HouseholdBudget.Service.Interfaces;

using Mapster;

namespace HouseholdBudget.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<IEnumerable<ExpenseDTO>> GetAllAsync()
        {
            var expenses = await _expenseRepository.GetAllAsync();
            return expenses.Adapt<IEnumerable<ExpenseDTO>>();
        }

        public async Task<ExpenseDTO?> GetByIdAsync(int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            return expense.Adapt<ExpenseDTO>();
        }

        public async Task AddAsync(ExpenseDTO expenseDto)
        {
            var expense = expenseDto.Adapt<Expense>();
            await _expenseRepository.AddAsync(expense);
        }

        public async Task UpdateAsync(ExpenseDTO expenseDto)
        {
            var expense = expenseDto.Adapt<Expense>();
            await _expenseRepository.UpdateAsync(expense);
        }

        public async Task DeleteAsync(int id)
        {
            await _expenseRepository.DeleteAsync(id);
        }
    }
}
