namespace HouseholdBudget.DTO.Authentication;

public abstract class AuthenticationRequestDTO
{
    public string Email { get; set; } = string.Empty;   
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthenticationCreateRequestDTO: AuthenticationRequestDTO
{

}

public sealed class AuthenticationLoginRequestDTO : AuthenticationRequestDTO
{

}