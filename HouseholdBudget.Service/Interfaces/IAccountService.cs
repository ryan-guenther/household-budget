using HouseholdBudget.DTO.Account;

namespace HouseholdBudget.Service.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountListResponseDTO>> GetAllAsync();
        Task<IEnumerable<AccountListResponseDTO>> AdminGetAllAsync();
        Task<AccountDetailResponseDTO?> GetByIdAsync(int id);
        Task<AccountDetailResponseDTO?> AdminGetByIdAsync(int id);
        Task<AccountDetailResponseDTO> AddAsync(AccountCreateRequestDTO accountDto);
        Task<AccountDetailResponseDTO> UpdateAsync(AccountUpdateRequestDTO accountDto);
        Task DeleteAsync(int id);
    }
}
