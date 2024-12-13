using HouseholdBudget.DTO.Authentication;

namespace HouseholdBudget.Service.Interfaces;
/// <summary>
/// Defines the contract for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    Task<string> RegisterAsync(AuthenticationCreateRequestDTO request);
    Task<string> LoginAsync(AuthenticationLoginRequestDTO request);
}