using HouseholdBudget.DTO.Authentication;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.BusinessLogic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HouseholdBudget.Domain;

namespace HouseholdBudget.Service;
/// <summary>
/// Provides authentication-related operations, including user registration and login.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(AuthenticationCreateRequestDTO request)
    {
        // Check if the email is already in use
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            string error = $"A user with the email {request.Email} already exists.";
            _logger.LogError(error);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Create the user
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            string error = $"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            _logger.LogError(error);
            throw new InvalidOperationException(error);
        }

        // Assign a default role to the user (optional)
        await _userManager.AddToRoleAsync(user, Roles.Definitions.User.Name);

        _logger.LogInformation($"Successful regisration completed for user email {user.Email}.");

        return "Registration successful.";
    }

    public async Task<string> LoginAsync(AuthenticationLoginRequestDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            string error = $"Invalid login attempt for email {request.Email}.";
            _logger.LogError(error);
            throw new UnauthorizedAccessException(error);
        }

        // Use SignInManager to verify credentials
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            string error = $"Invalid login attempt for email {request.Email}.";
            _logger.LogError(error);
            throw new UnauthorizedAccessException(error);
        }

        string jwtToken;

        try
        {
            // Generate and return the JWT token
            string? jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"];
            string? jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
            string? jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];
            int jwtExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? _configuration["Jwt:ExpiryMinutes"]);
            jwtToken = AuthenticationBL.GenerateJwtToken(user, jwtKey, jwtIssuer, jwtAudience, jwtExpiryMinutes);
        }
        catch (Exception ex)
        {
            string error = $"An error occurred while attempting to login for email {request.Email}.";
            _logger.LogError(error, ex.Message);
            throw new ApplicationException($"{error}: {ex.Message}", ex);
        }

        return jwtToken;
    }
}
